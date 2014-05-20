using System;

namespace Griffin.Container
{
    /// <summary>
    ///     The requested service has not been registerd.
    /// </summary>
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(Type serviceType, Exception inner)
            : base("Service not registered: " + serviceType.FullName, inner)
        {
            ServiceType = serviceType;
        }

        /// <summary>
        ///     Gets services that was requested
        /// </summary>
        public Type ServiceType { get; private set; }
    }
}