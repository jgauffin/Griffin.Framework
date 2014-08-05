using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DotNetCqs;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Queues commands and execute them in order in the background.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This bus stores all incoming commands in a queue. It then uses one or more Tasks to execute the incoming
    ///         commands.
    ///     </para>
    /// </remarks>
    public class QueuedCommandBus : ICommandBus, IDisposable, IApplicationService
    {
        private readonly ICommandBus _commandBus;
        private readonly IQueue<Command> _queue;
        private readonly ManualResetEventSlim _jobEvent = new ManualResetEventSlim(false);
        private readonly SemaphoreSlim _semaphore;
        private readonly int _workerCount;

        private bool _shutdown;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueuedCommandBus" /> class.
        /// </summary>
        /// <param name="queue">Used to store items before the command is executed.</param>
        /// <param name="commandBus">
        ///     Used to execute a command once it arrives to the beginning of the queue.
        /// </param>
        /// <param name="workerCount">Amount of commands that can be executed simultaneously.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     queue
        ///     or
        ///     commandBus
        /// </exception>
        public QueuedCommandBus(IQueue<Command> queue, ICommandBus commandBus, int workerCount)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (commandBus == null) throw new ArgumentNullException("commandBus");

            _queue = queue;
            _commandBus = commandBus;
            _workerCount = workerCount;
            _semaphore = new SemaphoreSlim(workerCount);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueuedCommandBus" /> class.
        /// </summary>
        /// <param name="commandBus">
        ///     Used to execute a command once it arrives to the beginning of the queue.
        /// </param>
        /// <exception cref="System.ArgumentNullException">commandBus</exception>
        /// <remarks>
        ///     <para>
        ///         Uses a <![CDATA[ConcurrentQueue<T>]]> and 10 workers.
        ///     </para>
        /// </remarks>
        public QueuedCommandBus(ICommandBus commandBus)
        {
            if (commandBus == null) throw new ArgumentNullException("commandBus");

            _queue = new Griffin.MemoryQueue<Command>();
            _commandBus = commandBus;
            _workerCount = 5;
            _semaphore = new SemaphoreSlim(5);
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
        ///     Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>
        ///     Task which completes once the command has been delivered (and NOT when it has been executed).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">command</exception>
        /// <remarks>
        ///     <para>
        ///         The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed
        ///         just because this method returns. That just means
        ///         that the command have been successfully delivered (to a queue or another process etc) for execution.
        ///     </para>
        /// </remarks>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            await _queue.EnqueueAsync(command);
            if (await _semaphore.WaitAsync(0))
                await Task.Run((Func<Task>) ExecuteFirstTask);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        ///     A specific handler failed to consume the application event.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We will not try to invoke the event again as one or more handlers may have consumed the event successfully.
        ///     </para>
        /// </remarks>
        public event EventHandler<CommandHandlerFailedEventArgs> HandlerFailed = delegate { };

        /// <summary>
        ///     Bus failed to invoke an event.
        /// </summary>
        public event EventHandler<BusFailedEventArgs> BusFailed = delegate { };

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
        /// </summary>
        /// <returns><c>true</c> if we found a command to execute.</returns>
        internal async Task<bool> ExecuteJobAsync()
        {
            var command = await _queue.DequeueAsync();
            if (command == null)
                return false;

            await _commandBus.ExecuteAsync((dynamic) command);

            return true;
        }

    }
}