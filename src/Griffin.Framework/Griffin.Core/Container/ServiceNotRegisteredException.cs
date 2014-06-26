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
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotRegisteredException"/> class.
        /// </summary>
        /// <param name="serviceType">Service that was not registered in the container.</param>
        /// <param name="inner">The inner exception.</param>
        public ServiceNotRegisteredException(Type serviceType, Exception inner)
            : base("Service not registered: " + serviceType.FullName, inner)
        {
            ServiceType = serviceType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotRegisteredException"/> class.
        /// </summary>
        /// <param name="serviceType">Service that was not registered in the container.</param>
        public ServiceNotRegisteredException(Type serviceType)
            : base("Service not registered: " + serviceType.FullName)
        {
            ServiceType = serviceType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotRegisteredException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
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