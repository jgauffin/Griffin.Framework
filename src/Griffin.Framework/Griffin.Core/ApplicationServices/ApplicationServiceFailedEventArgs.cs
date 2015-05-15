using System;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     A service failed (crashed or could not be started).
    /// </summary>
    /// <remarks>
    ///     Assign <see cref="CanContinue" /> to specify if we can continnue to check services.
    /// </remarks>
    public class ApplicationServiceFailedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BackgroundJobFailedEventArgs" /> class.
        /// </summary>
        /// <param name="applicationService">Service that failed</param>
        /// <param name="exception">Caught exception.</param>
        public ApplicationServiceFailedEventArgs(IApplicationService applicationService, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            ApplicationService = applicationService;
            Exception = exception;
        }

        /// <summary>
        ///     Service that we could not start (or restart).
        /// </summary>
        public IApplicationService ApplicationService { get; private set; }

        /// <summary>
        ///     Thrown exception
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     Continue to check the rest of our services.
        /// </summary>
        /// <value>
        ///     Default is <c>false</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// <c>true</c> means that we'll check all other services too. <c>false</c> means that we'll exit 
        /// this check iteration, wait <see cref="ApplicationServiceManager.CheckInterval"/> and then check
        /// all services again.
        /// </para>
        /// </remarks>
        public bool CanContinue { get; set; }
    }
}