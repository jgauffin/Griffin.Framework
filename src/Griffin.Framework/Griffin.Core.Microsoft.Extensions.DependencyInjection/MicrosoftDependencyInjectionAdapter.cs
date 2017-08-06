using Griffin.Container;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Core.Microsoft.Extensions.DependencyInjection
{
    public class MicrosoftDependencyInjectionAdapter : IContainer
    {
        private readonly IServiceProvider _services;

        public MicrosoftDependencyInjectionAdapter(IServiceProvider services)
        {
            _services = services;
        }

        public IContainerScope CreateScope()
        {
            return new MicrosoftDependencyInjectionScopeAdapter(_services.CreateScope());
        }

        public object Resolve(Type service)
        {
            return _services.GetService(service);
        }

        public TService Resolve<TService>()
        {
            return _services.GetService<TService>();
        }

        public IEnumerable<object> ResolveAll(Type service)
        {
            return _services.GetServices(service);
        }

        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _services.GetServices<TService>();
        }
    }
}
