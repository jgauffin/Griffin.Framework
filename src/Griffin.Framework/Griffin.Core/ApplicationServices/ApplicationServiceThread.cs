using System;
using System.Threading;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     An application service which runs inside a thread.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Make sure that you wait on the wait handle to support graceful shutdowns.
    ///     </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [ContainerService(Lifetime = Lifetime.SingleInstance)]
    /// public class TestAppService : ApplicationServiceThread
    /// {
    /// 	public TestAppService()
    /// 	{
    /// 		
    /// 	}
    /// 
    /// 	protected void Run(WaitHandle shutdownHandle)
    /// 	 {
    /// 		 while (true)
    /// 		 {
    /// 			 try
    /// 			 {
    /// 				 // pause 100ms between each loop iteration.
    /// 				 // you can specify 0 too
    /// 				 if (shutdownHandle.WaitOne(100))
    /// 					 break;
    /// 	 
    /// 				 // do actual logic here.
    /// 			 } 
    /// 			 catch (Exception ex)
    /// 			 {
    /// 				 // shutdown thread if it's a DB exception
    /// 				 // thread will be started again by the ApplicationServiceManager
    /// 				 if (ex is DataException)
    /// 					 throw;
    /// 	 
    /// 				 _log.Error("Opps", ex);
    /// 			 }
    /// 		 }
    /// 	 }
    /// }
    /// </code>
    /// </example>
    public abstract class ApplicationServiceThread : IGuardedService
    {
        private readonly ManualResetEvent _shutDownEvent = new ManualResetEvent(false);
        private Thread _workThread;
        private Action<string> _logFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServiceThread"/> class.
        /// </summary>
        protected ApplicationServiceThread()
        {
            IsRunning = false;
            StopWaitTime = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        ///     Failed to execute service.
        /// </summary>
        /// <remarks>
        /// <para>The string is the log entry to write to the log.</para>
        /// </remarks>
        public event EventHandler<ApplicationServiceFailedEventArgs> Failed = delegate{};

        /// <summary>
        ///     How long to wait on the thread for completion before terminating it.
        /// </summary>
        public TimeSpan StopWaitTime { get; set; }

        /// <summary>
        ///     Returnerar <c>true</c> om tjänsten är uppe och snurrar.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Starta vad det nu är som tjänsten hanterar.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("Can not start a started thread.");

            IsRunning = true;
            _workThread = new Thread(OnDoWork);
            _workThread.Start();
        }

        /// <summary>
        ///     Stäng ned det som tjänsten hanterar
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("Can not stop a thread that have not been started.");

            _shutDownEvent.Set();
            try
            {
                if (!_workThread.Join(StopWaitTime))
                {
                    _workThread.Abort();
                    Failed(this,
                        new ApplicationServiceFailedEventArgs(this,
                            new Exception("Failed to stop thread '" + GetType().FullName +
                                          "' within the given timespan of " +
                                          StopWaitTime)));
                }
            }
            finally
            {
                IsRunning = false;
                _workThread = null;
                _shutDownEvent.Reset();
            }
        }

        /// <summary>
        ///     Run your logic.
        /// </summary>
        /// <param name="shutdownHandle">Being triggered when your method should stop running.</param>
        /// <example>
        ///     <code>
        /// protected void Run(WaitHandle shutdownHandle)
        /// {
        ///     while (true)
        ///     {
        ///         try
        ///         {
        ///             // pause 100ms between each loop iteration.
        ///             // you can specify 0 too
        ///             if (shutdownHandle.WaitOne(100))
        ///                 break;
        /// 
        ///             // do actual logic here.
        ///         } 
        ///         catch (Exception ex)
        ///         {
        ///             // shutdown thread if it's a DB exception
        ///             // thread will be started again by the ApplicationServiceManager
        ///             if (ex is DataException)
        ///                 throw;
        /// 
        ///             _log.Error("Opps", ex);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        protected abstract void Run(WaitHandle shutdownHandle);

        private void OnDoWork()
        {
            try
            {
                Run(_shutDownEvent);
            }
            catch (Exception exception)
            {
                Failed(this, new ApplicationServiceFailedEventArgs(this, exception));
            }
            finally
            {
                IsRunning = false;
            }
        }
    }

}