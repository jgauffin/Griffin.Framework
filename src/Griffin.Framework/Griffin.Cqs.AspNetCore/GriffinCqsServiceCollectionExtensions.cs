using Griffin.Core.Microsoft.Extensions.DependencyInjection;
using Griffin.Cqs.InversionOfControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GriffinCqsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Griffin.Cqs.AspNetCore services to the IServiceCollection.
        /// You do not have to use the scanAssemblies parameter but then you have to register the Handlers yourself.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="scanAssemblies">will be scanned for <see cref="Griffin.Container.ContainerServiceAttribute"/> and registers all these classes as service</param>
        public static void AddGriffinCqs(this IServiceCollection services, params Assembly[] scanAssemblies)
        {
            services.AddSingleton(s =>
            {
                var builder = new IocBusBuilder(new MicrosoftDependencyInjectionAdapter(s));
                return builder.BuildMessageProcessor();
            });
            services.RegisterServices(scanAssemblies);
        }
    }
}
