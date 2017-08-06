using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Griffin.Container;

namespace Griffin.Core.Microsoft.Extensions.DependencyInjection
{
    class MicrosoftDependencyInjectionScopeAdapter : IContainerScope
    {
        private readonly IServiceScope _serviceScope;

        public MicrosoftDependencyInjectionScopeAdapter(IServiceScope serviceScope)
        {
            _serviceScope = serviceScope;
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public object Resolve(Type service)
        {
            return _serviceScope.ServiceProvider.GetService(service);
        }

        public TService Resolve<TService>()
        {
            return _serviceScope.ServiceProvider.GetService<TService>();
        }

        public IEnumerable<object> ResolveAll(Type service)
        {
            return _serviceScope.ServiceProvider.GetServices(service);
        }

        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _serviceScope.ServiceProvider.GetServices<TService>();
        }
    }
}
