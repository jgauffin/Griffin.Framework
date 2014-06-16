using System;
using System.Threading;
using Griffin.Logging;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     An application service which runs within a timer
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Do note that this implementation do not wait on completion when being shut down using <c>Stop()</c>.
    ///     </para>
    /// </remarks>
    public abstract class ApplicationServiceTimer : IApplicationService
    {
        private readonly Timer _timer;
        private ILogger _logger = LogManager.GetLogger<ApplicationServiceTimer>();
        private Action<string> _logFunc;

        protected ApplicationServiceTimer()
        {
            _timer = new Timer(OnTimer);
            FirstInterval = TimeSpan.FromMilliseconds(500);
            Interval = TimeSpan.FromSeconds(5);
            LogFunc = s => _logger.Error(GetType().FullName + " failed: " + s);
        }

        /// <summary>
        ///     Can be used to log errors to a log file
        /// </summary>
        public Action<string> LogFunc
        {
            get { return _logFunc; }
            set
            {
                if (value == null)
                    _logFunc = s => _logger.Error(GetType().FullName + " failed: " + s);
                else
                    _logFunc = value;
            }
        }

        /// <summary>
        ///     Interval before the first invocation
        /// </summary>
        /// <value>
        ///     Default is 500ms.
        /// </value>
        public TimeSpan FirstInterval { get; set; }

        /// <summary>
        ///     Interval *between* every invocation
        /// </summary>
        /// <value>
        ///     Default is 5 seconds
        /// </value>
        public TimeSpan Interval { get; set; }

        /// <summary>
        ///     Returnerar <c>true</c> om tjänsten är uppe och snurrar.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Starts the internal timer.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("Can not start a running timer");

            _timer.Change(FirstInterval, Interval);
            IsRunning = true;
        }

        /// <summary>
        ///     Stop the internal timer.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
        }

        /// <summary>
        ///     Used to do work periodically.
        /// </summary>
        /// <remarks>
        ///     Invoked every time the timer does an iteration. The interval is configured by <see cref="FirstInterval" /> and
        ///     <see cref="Interval" />. The intervals
        ///     are paused during the execution of <c>Execute()</c> so that your method is not invoked twice if it doesn't complete
        ///     within the specified interval.
        /// </remarks>
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
        ///             if (shutdownHandle.Wait(100))
        ///                 break;
        /// 
        ///             // do actual logic here.
        ///         } 
        ///         catch (Exception ex)
        ///         {
        ///             // shutdown thread if it's a DB exception
        ///             // thread will be started again by the ApplicationServiceManager
        ///             if (Exception is DataException)
        ///                 throw;
        /// 
        ///             _log.Error("Opps", ex);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        protected abstract void Execute();

        private void OnTimer(object state)
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Execute();
            }
            catch (Exception exception)
            {
                LogFunc(exception.ToString());
            }
            finally
            {
                if (IsRunning)
                {
                    _timer.Change(Interval, TimeSpan.FromMilliseconds(-1));
                }
            }
        }
    }
}