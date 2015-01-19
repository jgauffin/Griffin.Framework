using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.ApplicationServices;
using Griffin.Container;
using Griffin.IO;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Event bus implementation using autofac as a container.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This bus will store all events on disk and then execute them on a background thread. This means that the events will be
    ///         executed even if the application
    ///         crashes (unless the application crashes during the actual execution of the event).
    ///     </para>
    /// </remarks>
    public class QueuedEventBus : IEventBus, IApplicationService, IDisposable
    {
        private readonly IQueue<ApplicationEvent> _queue;
        private readonly IEventBus _innerBus;
        private readonly ManualResetEventSlim _jobEvent = new ManualResetEventSlim(false);
        private readonly SemaphoreSlim _semaphore;
        private readonly int _workerCount;

        private bool _shutdown;


        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedEventBus" /> class.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="innerBus">Bus used once it's time for an event to be executed.</param>
        /// <param name="workerCount"></param>
        /// <exception cref="System.ArgumentNullException">
        /// queue
        /// or
        /// innerBus
        /// </exception>
        public QueuedEventBus(IQueue<ApplicationEvent> queue, IEventBus innerBus, int workerCount)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (innerBus == null) throw new ArgumentNullException("innerBus");

            _queue = queue;
            _innerBus = innerBus;
            var iocBus = _innerBus as IocEventBus;
            if (iocBus != null)
            {
                iocBus.EventPublished += OnDelegateEventPublished;
                iocBus.HandlerFailed += OnDelegateHandlerFailed;
            }
            _workerCount = workerCount;
            _semaphore = new SemaphoreSlim(workerCount);
        }

        private void OnDelegateHandlerFailed(object sender, EventHandlerFailedEventArgs e)
        {
            HandlerFailed(this, e);
        }

        private void OnDelegateEventPublished(object sender, EventPublishedEventArgs e)
        {
            EventPublished(this, e);
        }

        /// <summary>
        ///     Event have been executed and the scope will be disposed after this event has been triggered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can use this event to save a Unit Of Work or similar.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// eventBus.EventPublished += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<EventPublishedEventArgs> EventPublished = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedEventBus" /> class.
        /// </summary>
        /// <param name="innerBus">Bus used once it's time for an event to be executed.</param>
        /// <param name="workerCount"></param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        /// <remarks>
        /// Uses <![CDATA[ConcurrentQueue<T>]]> to store events before they are executed.
        /// </remarks>
        public QueuedEventBus(IEventBus innerBus, int workerCount)
            : this(new Griffin.MemoryQueue<ApplicationEvent>(), innerBus, workerCount)
        {
        }

        /// <summary>
        ///     Publish a new application event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event to publish.</typeparam>
        /// <param name="e">Event to publish, must be serializable.</param>
        /// <returns>
        ///     Task triggered once the event has been delivered.
        /// </returns>
        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e)
            where TApplicationEvent : ApplicationEvent
        {
            await _queue.EnqueueAsync(e);
            if (await _semaphore.WaitAsync(0))
            {
                //TODO: Should await instead?
#pragma warning disable 4014
                Task.Run((Func<Task>)ExecuteFirstTask);
#pragma warning restore 4014
            }
        }

        /// <summary>
        ///     Start bus (required to start processing queued commands)
        /// </summary>
        public void Start()
        {
            _shutdown = false;
        }

        /// <summary>
        ///     Stop processing bus (will wait for the current command to be completed before shutting down)
        /// </summary>
        public void Stop()
        {
            _shutdown = true;
            _jobEvent.Set();
            for (var i = 0; i < _workerCount; i++)
            {
                _semaphore.Wait(1000);
            }
        }



        /// <summary>
        ///     A specific handler failed to consume the application event.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We will not try to invoke the event again as one or more handlers may have consumed the event successfully.
        ///     </para>
        /// </remarks>
        public event EventHandler<EventHandlerFailedEventArgs> HandlerFailed = delegate { };

        /// <summary>
        ///     Bus failed to invoke an event.
        /// </summary>
        public event EventHandler<BusFailedEventArgs> BusFailed = delegate { };

        /// <summary>
        /// 
        /// </summary>
        /// <returns><c>true</c> </returns>
        internal async Task<bool> ExecuteJobAsync()
        {
            var appEvent = await _queue.DequeueAsync();
            if (appEvent == null)
                return false;

            await _innerBus.PublishAsync((dynamic)appEvent);
            return true;
        }

        internal async Task ExecuteFirstTask()
        {
            while (true)
            {
                if (_shutdown)
                    return;

                try
                {
                    var result = await ExecuteJobAsync();
                    if (!result)
                        break;
                }
                catch (Exception exception)
                {
                    BusFailed(this, new BusFailedEventArgs(exception));
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}