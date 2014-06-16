using System;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Could not start (or restart) a service.
    /// </summary>
    /// <remarks>
    ///     Assign <see cref="CanContinue" /> to specify if we can continnue to check services.
    /// </remarks>
    public class ApplicationServiceFailedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BackgroundJobFailedEventArgs" /> class.
        /// </summary>
        /// <param name="applicationService">Tjänsten som inte gick att starta igen</param>
        /// <param name="exception">Undantaget som kastades.</param>
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
        public bool CanContinue { get; set; }
    }
}