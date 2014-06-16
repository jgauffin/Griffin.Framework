using System;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Failed to start a service.
    /// </summary>
    /// <seealso cref="ApplicationServiceManager" />
    public class StartServiceException : Exception
    {
        /// <summary>
        /// </summary>
        /// <param name="service">Service that failed</param>
        /// <param name="exception">Exception which prevented the service from starting.</param>
        public StartServiceException(IApplicationService service, Exception exception)
            : base("Failed to start '" + service + "'.", exception)
        {
            if (service == null) throw new ArgumentNullException("service");
            Service = service;
        }

        /// <summary>
        ///     Gets service which could not be started
        /// </summary>
        public IApplicationService Service { get; private set; }
    }
}