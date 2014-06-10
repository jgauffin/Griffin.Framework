using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Event bus implementation using autofac as a container.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This bus stores all incoming commands in a queue. It then uses one or more Tasks to execute the incoming
    ///         commands.
    ///     </para>
    /// </remarks>
    public class ContainerCommandBus : ICommandBus, IDisposable, IApplicationService
    {
        private readonly IContainer _container;
        private readonly ManualResetEventSlim _jobEvent = new ManualResetEventSlim(false);
        private readonly IQueue<Command> _queue;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _workerCount;

        private bool _shutdown;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContainerCommandBus" /> class.
        /// </summary>
        /// <param name="queue">Used to store items before the command is executed.</param>
        /// <param name="container">
        ///     Used for service location (to lookup <![CDATA[ICommandHandler<T>]]>).
        /// </param>
        /// <param name="workerCount">Amount of commands that can be executed simultaneously.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     queue
        ///     or
        ///     container
        /// </exception>
        public ContainerCommandBus(IQueue<Command> queue, IContainer container, int workerCount)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (container == null) throw new ArgumentNullException("container");

            _queue = queue;
            _container = container;
            _workerCount = workerCount;
            _semaphore = new SemaphoreSlim(workerCount);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContainerCommandBus" /> class.
        /// </summary>
        /// <param name="container">
        ///     Used for service location (to lookup <![CDATA[ICommandHandler<T>]]>).
        /// </param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        /// <remarks>
        ///     <para>
        ///         Uses a <![CDATA[ConcurrentQueue<T>]]> and 10 workers.
        ///     </para>
        /// </remarks>
        public ContainerCommandBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            _queue = new MemoryQueue<Command>();
            _container = container;
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

            using (var scope = _container.CreateScope())
            {
                var type = typeof (ICommandHandler<>).MakeGenericType(command.GetType());
                var handlers = scope.ResolveAll(type).ToArray();

                if (handlers.Length == 0)
                    throw new CqsHandlerMissingException(command.GetType());
                if (handlers.Length != 1)
                    throw new OnlyOneHandlerAllowedException(command.GetType());

                try
                {
                    var method = type.GetMethod("ExecuteAsync");
                    var task = (Task) method.Invoke(handlers[0], new object[] {command});
                    await task;
                }
                catch (Exception exception)
                {
                    if (exception is TargetInvocationException)
                        exception = exception.InnerException;
                    HandlerFailed(this, new CommandHandlerFailedEventArgs(command, handlers[0], exception));
                }
            }

            return true;
        }

        private void OnExecuteCommand()
        {
            while (true)
            {
                _jobEvent.Wait(1000);
                if (_shutdown)
                    return;

                try
                {
                    var task = ExecuteJobAsync();
                    task.Wait();
                    if (!task.Result)
                        _jobEvent.Reset();
                }
                catch (Exception exception)
                {
                    BusFailed(this, new BusFailedEventArgs(exception));
                }
            }
        }
    }
}