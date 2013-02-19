using System;

namespace Griffin.Framework.InversionOfControl
{
    /// <summary>
    /// Failed to resolve a service.
    /// </summary>
    public class ServiceNotRegisteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotRegisteredException"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ServiceNotRegisteredException(Type service)
            : base("Failed to resolve '" + service.FullName + "'. Have you registered it in the container? Remember that you should depend on abstractions and not concretes.")
        {

        }
    }
}