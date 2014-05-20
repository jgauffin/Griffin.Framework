using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Griffin.Container;

namespace Griffin.Core.Autofac
{
    public class AutofacScope : IContainerScope
    {
        private readonly ILifetimeScope _scope;

        public AutofacScope(ILifetimeScope scope)
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