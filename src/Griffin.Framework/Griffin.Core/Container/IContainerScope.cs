using System;
using System.Collections.Generic;

namespace Griffin.Container
{
    /// <summary>
    /// Represents a container with a limited lifetime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A child container stores all scoped objects in a list and will dispose all resolved objects when the scope is being disposed.
    /// </para>
    /// </remarks>
    public interface IContainerScope : IDisposable
    {
        /// <summary>
        ///     Resolve the last registered implementation of a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation for</typeparam>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        TService Resolve<TService>();

        /// <summary>
        ///     Resolve the last registered implementation of a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation for.</param>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        /// <exception cref="ArgumentNullException">service</exception>
        object Resolve(Type service);

        /// <summary>
        ///     Resolve all implementations of a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation(s) for</typeparam>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        IEnumerable<TService> ResolveAll<TService>();

        /// <summary>
        ///     Resolve all implementations of a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation(s) for</param>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        /// <exception cref="ArgumentNullException">service</exception>
        IEnumerable<object> ResolveAll(Type service);
    }
}