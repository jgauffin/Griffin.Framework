using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Griffin.Container;

namespace Griffin.Core.Autofac
{
    /// <summary>
    /// Adapter for autofac and the griffin child container contract.
    /// </summary>
    public class AutofacScopeAdapter : IContainerScope
    {
        private readonly ILifetimeScope _scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacScopeAdapter"/> class.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <exception cref="System.ArgumentNullException">scope</exception>
        public AutofacScopeAdapter(ILifetimeScope scope)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            _scope = scope;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _scope.Dispose();
        }

        /// <summary>
        ///     Resolve the last registered implementation of a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation for</typeparam>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public TService Resolve<TService>()
        {
            try
            {
                return _scope.Resolve<TService>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(typeof (TService), ex);
            }
            catch (DependencyResolutionException ex)
            {
                throw new DependencyMissingException(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Resolve the last registered implementation of a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation for.</param>
        /// <returns>object that implements the specified service</returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        /// <exception cref="ArgumentNullException">service</exception>
        public object Resolve(Type service)
        {
            try
            {
                return _scope.Resolve(service);
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(service, ex);
            }
            catch (DependencyResolutionException ex)
            {
                throw new DependencyMissingException(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Resolve all implementations of a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation(s) for</typeparam>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public IEnumerable<TService> ResolveAll<TService>()
        {
            try
            {
                return _scope.Resolve<IEnumerable<TService>>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(typeof (TService), ex);
            }
            catch (DependencyResolutionException ex)
            {
                throw new DependencyMissingException(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Resolve all implementations of a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation(s) for</param>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        /// <exception cref="ArgumentNullException">service</exception>
        public IEnumerable<object> ResolveAll(Type service)
        {
            try
            {
                var type = typeof(IEnumerable<>).MakeGenericType(service);
                return (IEnumerable<object>)_scope.Resolve(type);
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(service, ex);
            }
            catch (DependencyResolutionException ex)
            {
                throw new DependencyMissingException(ex.Message, ex);
            }
        }
    }
}