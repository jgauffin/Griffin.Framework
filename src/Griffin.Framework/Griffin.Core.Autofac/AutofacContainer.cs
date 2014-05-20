using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Griffin.Container;
using IContainer = Griffin.Container.IContainer;

namespace Griffin.Core.Autofac
{
    public class AutofacContainer : IContainer
    {
        private readonly global::Autofac.IContainer _container;

        public AutofacContainer(global::Autofac.IContainer container)
        {
            _container = container;
        }

        public IContainerScope CreateScope()
        {
            return new AutofacScope(_container.BeginLifetimeScope());
        }

        public TService Resolve<TService>()
        {
            try
            {
                return _container.Resolve<TService>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(typeof(TService), ex);
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

        public IEnumerable<TService> ResolveAll<TService>()
        {
            try
            {
                return _container.Resolve<IEnumerable<TService>>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotRegisteredException(typeof(TService), ex);
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
                return (IEnumerable<object>)_container.Resolve(type);
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
