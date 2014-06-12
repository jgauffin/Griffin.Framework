using System;
using System.Runtime.Serialization;

namespace Griffin.Container
{
    /// <summary>
    ///     The requested service has not been registerd.
    /// </summary>
    [Serializable]
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(Type serviceType, Exception inner)
            : base("Service not registered: " + serviceType.FullName, inner)
        {
            ServiceType = serviceType;
        }

        public ServiceNotRegisteredException(Type serviceType)
            : base("Service not registered: " + serviceType.FullName)
        {
            ServiceType = serviceType;
        }

        protected ServiceNotRegisteredException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }


        /// <summary>
        ///     Gets services that was requested
        /// </summary>
        public Type ServiceType { get; private set; }
    }
}