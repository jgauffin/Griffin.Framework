using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Container;
using Griffin.Logging;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Used to execute all classes that implement <see cref="IBackgroundJob" />. The jobs are executed in parallel.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This implementation uses your inversion of control container via the interface
    ///         <see cref="Griffin.Container.IContainer" />.. A new scope (
    ///         <see
    ///             cref="IContainerScope" />
    ///         ) is created for every time a job is executed.
    ///     </para>
    ///     <para>
    ///         By subscribing on the event <see cref="ScopeClosing" /> you can for instance commit an unit of work every time a
    ///         job have been executed.
    ///     </para>
    ///     <para>
    ///         To be able to run every job in isolation (in an own scope) we need to be able to find all background jobs. To
    ///         do that a temporary scope
    ///         is created during startup to resolve all jobs (<c><![CDATA[scope.ResolveAll<IBackgroundJob>()]]></c>. The jobs
    ///         are not invoked but only located so that we can map all background job types. Then later
    ///         when we are going to execute each job we use <c><![CDATA[scope.Resolve(jobType)]]></c> for every job that is
    ///         about to be executed.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         Example for a windows service class:
    ///     </para>
    ///     <code>
    /// public class Service1 : ServiceBase
    /// {
    ///     BackgroundJobManager _jobInvoker;
    ///     IContainer  _container;
    /// 
    ///     public Service1()
    ///     {
    ///         _serviceLocator = CreateContainer();
    /// 
    ///         _jobInvoker = new BackgroundJobManager(_container);
    ///         _jobInvoker.ScopeClosing += OnScopeClosing;
    ///     }
    /// 
    ///     public override OnStart(string[] argv)
    ///     {
    ///         _jobInvoker.Start();
    ///     }
    /// 
    ///     public override OnStop()
    ///     {
    ///         _jobInvoker.Stop();
    ///     }
    /// 
    ///     public void CreateContainer()
    ///     {
    ///         // either create your container directly
    ///         // or use the composition root pattern.
    ///     }
    /// 
    ///     // so that we can commit the transaction
    ///     // event will not be invoked if something fails.
    ///     public void OnScopeClosing(object sender, ScopeCreatedEventArgs e)
    ///     {
    ///         <![CDATA[e.Scope.Resolve<IUnitOfWork>().SaveChanges();]]>
    ///     }
    /// }
    /// </code>
    /// </example>
    public class BackgroundJobManager : IDisposable
    {
        private readonly IContainer _container;
        private readonly ILogger _logger = LogManager.GetLogger(typeof (BackgroundJobManager));
        private List<Type> _asyncJobTypes = new List<Type>();
        private List<Type> _syncJobTypes = new List<Type>();
        private Timer _timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BackgroundJobManager" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public BackgroundJobManager(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
            _timer = new Timer(OnExecuteJob, null, Timeout.Infinite, Timeout.Infinite);
            StartInterval = TimeSpan.FromSeconds(1);
            ExecuteInterval = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        ///     Amount of time before running the jobs for the first time
        /// </summary>
        public TimeSpan StartInterval { get; set; }

        /// <summary>
        ///     Amount of time that should be passed between every execution run.
        /// </summary>
        public TimeSpan ExecuteInterval { get; set; }

        /// <summary>
        /// Dont execute jobs in parallell (parallell executions can cause deadloks for DB jobs)
        /// </summary>
        public bool ExecuteSequentially { get; set; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }

        /// <summary>
        ///     A new scope has been created and the jobs are about to be executed.
        /// </summary>
        public event EventHandler<ScopeCreatedEventArgs> ScopeCreated = delegate { };


        /// <summary>
        ///     A job have finished executing.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will not be invoked if <see cref="JobFailed" /> event sets <c>CanContinue</c> to <c>false</c>.
        ///     </para>
        /// </remarks>
        public event EventHandler<ScopeClosingEventArgs> ScopeClosing = delegate { };

        /// <summary>
        ///     One of the jobs failed
        /// </summary>
        /// <remarks>
        ///     Use this event to determine if the rest of the jobs should be able to execute.
        /// </remarks>
        public event EventHandler<BackgroundJobFailedEventArgs> JobFailed = delegate { };

        /// <summary>
        ///     Start executing jobs (once the <see cref="StartInterval" /> have been passed).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will do one initial resolve on all jobs in the container to be able to discover their <c>Type</c>. Without this
        ///         check it would not be
        ///         possible to run each job in an isolated scope.
        ///     </para>
        /// </remarks>
        public void Start()
        {
            using (var scope = _container.CreateScope())
            {
                var jobs = scope.ResolveAll<IBackgroundJob>();
                _syncJobTypes = jobs.Select(x => x.GetType()).ToList();
            }

            using (var scope = _container.CreateScope())
            {
                var jobs = scope.ResolveAll<IBackgroundJobAsync>();
                _asyncJobTypes = jobs.Select(x => x.GetType()).ToList();
            }

            _timer.Change(StartInterval, ExecuteInterval);
        }

        /// <summary>
        ///     Stop executing jobs.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnExecuteJob(object state)
        {
            Task allTask = null;
            try
            {
                if (ExecuteSequentially)
                {
                    foreach (var jobType in _syncJobTypes)
                    {
                        ExecuteSyncJob(jobType);
                    }

                    foreach (var jobType in _asyncJobTypes)
                    {
                        ExecuteAsyncJob(jobType).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    Parallel.ForEach(_syncJobTypes, ExecuteSyncJob);
                    var tasks = _asyncJobTypes.Select(ExecuteAsyncJob);
                    allTask = Task.WhenAll(tasks);
                    allTask.Wait();
                }

            }
            catch (Exception exception)
            {
                _logger.Error("failed to execute jobs.", exception);
                if (allTask != null)
                    JobFailed(this, new BackgroundJobFailedEventArgs(new NoJob(GetType(), allTask.Exception), exception));
                else
                    JobFailed(this, new BackgroundJobFailedEventArgs(new NoJob(GetType(), exception), exception));
            }
        }

        private void ExecuteJobs()
        {

        }

        private void ExecuteSyncJob(Type jobType)
        {
            IBackgroundJob job = null;
            try
            {
                using (var scope = _container.CreateScope())
                {
                    try
                    {
                        job = (IBackgroundJob)scope.Resolve(jobType);
                        if (job == null)
                            throw new InvalidOperationException(string.Format("Failed to resolve job type '{0}'.", jobType.FullName));

                        ScopeCreated(this, new ScopeCreatedEventArgs(scope));
                        job.Execute();
                        ScopeClosing(this, new ScopeClosingEventArgs(scope, true));
                    }
                    catch (Exception exception)
                    {
                        var args = new BackgroundJobFailedEventArgs(job ?? new NoJob(jobType, exception), exception);
                        JobFailed(this, args);
                        ScopeClosing(this, new ScopeClosingEventArgs(scope, false) { Exception = exception });
                    }
                }
            }
            catch (Exception exception)
            {
                JobFailed(this, new BackgroundJobFailedEventArgs(new NoJob(jobType, exception), exception));
                _logger.Error("Failed to execute job: " + job, exception);
            }
        }
        private async Task ExecuteAsyncJob(Type jobType)
        {
            IBackgroundJobAsync job = null;
            try
            {
                using (var scope = _container.CreateScope())
                {
                    try
                    {
                        job = (IBackgroundJobAsync) scope.Resolve(jobType);
                        ScopeCreated(this, new ScopeCreatedEventArgs(scope));
                        await job.ExecuteAsync();
                        ScopeClosing(this, new ScopeClosingEventArgs(scope, true));
                    }
                    catch (Exception exception)
                    {
                        var args = new BackgroundJobFailedEventArgs((object)job ?? new NoJob(jobType, exception),
                            exception);
                        JobFailed(this, args);
                        ScopeClosing(this, new ScopeClosingEventArgs(scope, false) {Exception = exception});
                    }
                }
            }
            catch (Exception exception)
            {
                JobFailed(this, new BackgroundJobFailedEventArgs(new NoJob(jobType, exception), exception));
                _logger.Error("Failed to execute job: " + job, exception);
            }
        }

        /// <summary>
        /// Used in the events when a job can not be constructed.
        /// </summary>
        public class NoJob : IBackgroundJob
        {
            private readonly Exception _exception;

            /// <summary>
            /// Initializes a new instance of the <see cref="NoJob"/> class.
            /// </summary>
            /// <param name="jobType">Type of the job.</param>
            /// <param name="exception">The exception.</param>
            /// <exception cref="System.ArgumentNullException">exception</exception>
            public NoJob(Type jobType, Exception exception)
            {
                if (exception == null) throw new ArgumentNullException("exception");
                JobType = jobType ?? GetType();
                _exception = exception;
            }

            /// <summary>
            /// Job that could not be created
            /// </summary>
            public Type JobType { get; set; }

            /// <summary>
            /// Exception that prevents job from being created.
            /// </summary>
            public Exception Exception
            {
                get { return _exception; }
            }

            /// <summary>
            ///     NOOP
            /// </summary>
            public void Execute()
            {
            }

            /// <summary>
            ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
            /// </summary>
            /// <returns>
            ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
            /// </returns>
            public override string ToString()
            {
                return string.Format(
                        "scope.ResolveAll<IBackgroundJob>() failed for {0}. Check exception property for more information. '{1}'",
                        JobType.FullName, Exception.Message);
            }
        }
    }
}