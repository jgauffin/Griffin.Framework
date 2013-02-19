using System;
using System.Collections.Generic;

namespace Griffin.Framework.InversionOfControl
{
    /// <summary>
    /// All containers implement the service locator pattern.
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Resolve a service.
        /// </summary>
        /// <typeparam name="T">Requested service</typeparam>
        /// <returns>object which implements the service.</returns>
        /// <exception cref="ServiceNotRegisteredException">The service has not been registered in the container.</exception>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolve a service
        /// </summary>
        /// <param name="service">Requested service</param>
        /// <returns>object which implements the service</returns>
        /// <exception cref="ServiceNotRegisteredException">The service has not been registered in the container.</exception>
        object Resolve(Type service);

        /// <summary>
        /// Resolve a service
        /// </summary>
        /// <param name="service">Requested service</param>
        /// <param name="instance">Service if found</param>
        /// <returns><c>true</c> if service was found; otherwise <c>false</c>.</returns>
        bool TryResolve(Type service, out object instance);

        /// <summary>
        /// Resolve all found implementations.
        /// </summary>
        /// <typeparam name="T">Requested service</typeparam>
        /// <returns>objects which implements the service (or an empty list).</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Resolve all found implementations.
        /// </summary>
        /// <param name="service">Service to find</param>
        /// <returns>objects which implements the service (or an empty list).</returns>
        IEnumerable<object> ResolveAll(Type service);
    }
}