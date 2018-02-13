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
    public abstract class ApplicationServiceTimer : IGuardedService
    {
        private readonly Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServiceTimer"/> class.
        /// </summary>
        protected ApplicationServiceTimer()
        {
            _timer = new Timer(OnTimer);
            FirstInterval = TimeSpan.FromMilliseconds(500);
            Interval = TimeSpan.FromSeconds(5);
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
        ///     Returns <c>true</c> if the timer is running.
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
            if (!IsRunning)
                throw new InvalidOperationException("Can not stop a service which is not running");

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
        }

        /// <summary>
        /// Service failed to execute.
        /// </summary>
        public event EventHandler<ApplicationServiceFailedEventArgs> Failed=delegate {};

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
        /// protected override void Execute()
        /// {
        ///    //Do some work.
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
                Failed(this, new ApplicationServiceFailedEventArgs(this, exception));
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