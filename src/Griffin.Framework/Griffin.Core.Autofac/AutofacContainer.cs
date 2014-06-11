using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Griffin.Container;
using IContainer = Griffin.Container.IContainer;
using ResolutionExtensions = Autofac.ResolutionExtensions;

namespace Griffin.Core.Autofac
{
    /// <summary>
    ///     Adapter between Autofac and the Griffin inversion of control contract.
    /// </summary>
    public class AutofacContainer : IContainer
    {
        private readonly global::Autofac.IContainer _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacContainer" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public AutofacContainer(global::Autofac.IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        ///     Created a child scope (i.e. a container with a lifetime that you control. Dispose the scope to clean up all
        ///     resolved services).
        /// </summary>
        /// <returns>
        ///     A child container (i.e. a lifetime scope)
        /// </returns>
        public IContainerScope CreateScope()
        {
            return new AutofacScope(_container.BeginLifetimeScope());
        }

        /// <summary>
        ///     Resolve the last registered implementation for a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation for</typeparam>
        /// <returns>
        ///     object that implements the specified service
        /// </returns>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public TService Resolve<TService>()
        {
            try
            {
                return ResolutionExtensions.Resolve<TService>((IComponentContext) _container);
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
        ///     Resolve the last registered implementation for a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation for.</param>
        /// <returns>
        ///     object that implements the specified service
        /// </returns>
        /// <exception cref="System.ArgumentNullException">service</exception>
        /// <exception cref="ServiceNotRegisteredException">Failed to find an implementation for the service</exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public object Resolve(Type service)
        {
            if (service == null) throw new ArgumentNullException("service");
            try
            {
                return _container.Resolve(service);
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
        ///     Resolve all implementations for a service.
        /// </summary>
        /// <typeparam name="TService">Service that we want to get an implementation(s) for</typeparam>
        /// <returns>A list of implementations, or an empty list if no implementations are found.</returns>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public IEnumerable<TService> ResolveAll<TService>()
        {
            try
            {
                return _container.Resolve<IEnumerable<TService>>();
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
        ///     Resolve all implementations for a service.
        /// </summary>
        /// <param name="service">Service that we want to get an implementation(s) for</param>
        /// <returns>
        ///     A list of implementations, or an empty list if no implementations are found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">service</exception>
        /// <exception cref="ServiceNotRegisteredException"></exception>
        /// <exception cref="DependencyMissingException">A dependency was missing when constructing the service implementation.</exception>
        public IEnumerable<object> ResolveAll(Type service)
        {
            if (service == null) throw new ArgumentNullException("service");
            try
            {
                var type = typeof (IEnumerable<>).MakeGenericType(service);
                return (IEnumerable<object>) _container.Resolve(type);
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