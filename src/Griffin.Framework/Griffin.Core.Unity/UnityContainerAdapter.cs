using System;
using System.Collections.Generic;
using Griffin.Container;
using Microsoft.Practices.Unity;

namespace Griffin.Core.Unity
{
    /// <summary>
    /// Adapter between Unity and the Griffin.Framework contract.
    /// </summary>
    public class UnityContainerAdapter : IContainer
    {
        private readonly IUnityContainer _unity;

        public UnityContainerAdapter(IUnityContainer unity)
        {
            if (unity == null) throw new ArgumentNullException("unity");
            _unity = unity;
        }

        /// <summary>
        ///     Resolve the last registered implementation for a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation for</typeparam>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public TService Resolve<TService>()
        {
            if (!_unity.IsRegistered<TService>())
                throw new ServiceNotRegisteredException(typeof (TService));

            try
            {
                return _unity.Resolve<TService>();
            }
            catch (ResolutionFailedException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + typeof (TService) + "' due to a missing dependency. See inner exception.",
                    ex);
            }
            catch (Microsoft.Practices.ObjectBuilder2.DependencyMissingException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + typeof (TService) + "' due to a missing dependency. See inner exception.",
                    ex);
            }
        }

        /// <summary>
        ///     Resolve the last registered implementation for a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation for.</param>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public object Resolve(Type service)
        {
            if (service == null) throw new ArgumentNullException("service");
            try
            {
                return _unity.Resolve(service);
            }
            catch (ResolutionFailedException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + service + "' due to a missing dependency. See inner exception.", ex);
            }
            catch (Microsoft.Practices.ObjectBuilder2.DependencyMissingException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + service + "' due to a missing dependency. See inner exception.", ex);
            }
        }

        /// <summary>
        ///     Resolve all implementations for a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation(s) for</typeparam>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public IEnumerable<TService> ResolveAll<TService>()
        {
            try
            {
                return _unity.ResolveAll<TService>();
            }
            catch (ResolutionFailedException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + typeof (TService) + "' due to a missing dependency. See inner exception.",
                    ex);
            }
            catch (Microsoft.Practices.ObjectBuilder2.DependencyMissingException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + typeof (TService) + "' due to a missing dependency. See inner exception.",
                    ex);
            }
        }

        /// <summary>
        ///     Resolve all implementations for a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation(s) for</param>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public IEnumerable<object> ResolveAll(Type service)
        {
            try
            {
                return _unity.ResolveAll(service);
            }
            catch (ResolutionFailedException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + service + "' due to a missing dependency. See inner exception.", ex);
            }
            catch (Microsoft.Practices.ObjectBuilder2.DependencyMissingException ex)
            {
                throw new DependencyMissingException(
                    "Failed to resolve '" + service + "' due to a missing dependency. See inner exception.", ex);
            }
        }

        /// <summary>
        ///     Created a child scope (i.e. a container with a lifetime that you control. Dispose the scope to clean up all
        ///     resolved services).
        /// </summary>
        /// <returns>A child container (i.e. a lifetime scope)</returns>
        public IContainerScope CreateScope()
        {
            var scope = _unity.CreateChildContainer();
            return new UnityChildContainerAdapter(scope);
        }
    }
}