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
    public abstract class ApplicationServiceThread : IApplicationService
    {
        private readonly ManualResetEvent _shutDownEvent = new ManualResetEvent(false);
        private Thread _workThread;
        private Action<string> _logFunc;

        protected ApplicationServiceThread()
        {
            IsRunning = false;
            StopWaitTime = TimeSpan.FromMinutes(1);
            LogFunc = logEntry => { };
        }

        /// <summary>
        ///     Can be used to log errors to a log file
        /// </summary>
        /// <remarks>
        /// <para>The string is the log entry to write to the log.</para>
        /// </remarks>
        public Action<string> LogFunc
        {
            get { return _logFunc; }
            set { _logFunc = value ?? delegate { }; }
        }

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
                    if (LogFunc != null)
                        LogFunc("Failed to stop thread '" + GetType().FullName + "' within the given timespan of " +
                                StopWaitTime);
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
                LogFunc(exception.ToString());
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}