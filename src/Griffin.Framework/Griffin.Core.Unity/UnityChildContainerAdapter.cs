using System;
using System.Collections.Generic;
using Griffin.Container;
using Microsoft.Practices.Unity;

namespace Griffin.Core.Unity
{
    /// <summary>
    ///     Adapter for a Unity child container and the contract in Griffin.Framework.
    /// </summary>
    public class UnityChildContainerAdapter : IContainerScope
    {
        private readonly IUnityContainer _unityChildContainer;

        public UnityChildContainerAdapter(IUnityContainer unityChildContainer)
        {
            if (unityChildContainer == null) throw new ArgumentNullException("unityChildContainer");
            _unityChildContainer = unityChildContainer;
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
            if (!_unityChildContainer.IsRegistered<TService>())
                throw new ServiceNotRegisteredException(typeof (TService));

            try
            {
                return _unityChildContainer.Resolve<TService>();
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
                return _unityChildContainer.Resolve(service);
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
                return _unityChildContainer.ResolveAll<TService>();
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
                return _unityChildContainer.ResolveAll(service);
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
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _unityChildContainer.Dispose();
        }

        /// <summary>
        ///     Created a child scope (i.e. a container with a lifetime that you control. Dispose the scope to clean up all
        ///     resolved services).
        /// </summary>
        /// <returns>A child container (i.e. a lifetime scope)</returns>
        public IContainerScope CreateScope()
        {
            throw new NotImplementedException();
        }
    }
}