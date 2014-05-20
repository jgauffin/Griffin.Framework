using System;
using System.Collections.Generic;

namespace Griffin.Container
{
    /// <summary>
    ///     Contract for an inversion of control container
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation for</typeparam>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        TService Resolve<TService>();

        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <param name="service">Service that we want to get an implementation for.</param>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        object Resolve(Type service);

        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation(s) for</typeparam>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        IEnumerable<TService> ResolveAll<TService>();

        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <param name="service">Service that we want to get an implementation(s) for</param>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        IEnumerable<object> ResolveAll(Type service);

        /// <summary>
        ///     Created a child scope (i.e. a container with a lifetime that you control. Dispose the scope to clean up all
        ///     resolved services).
        /// </summary>
        /// <returns>A child container (i.e. a lifetime scope)</returns>
        IContainerScope CreateScope();
    }
}