using Griffin.Container;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Core.Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for microsoft dependency injection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Read the documentation for the attribute for more information about how the classes will be registered in the
    ///         container.
    ///     </para>
    ///     <para>First tag your classes with the attribute.</para>
    ///     <code>
    /// // This class will be registered in the container as <c>IMyDataFetcher</c>.
    /// [ContainerService]
    /// public class MyDataFetcher : IMyDataFetcher
    /// {
    /// }
    /// 
    /// // this class will be registered as itself, and live as long as the application
    /// [ContainerService(ContainerLifetime.SingleInstance)]
    /// public class UserCache
    /// {
    /// }
    /// </code>
    ///     <para>
    ///         Next you register them in microsoft dependency injection:
    ///     </para>
    ///     <code>
    /// 
    /// var cb = new IServiceCollection();
    /// cb.RegisterServices(Assembly.GetExecutingAssembly());
    /// 
    /// var container = cb.Build();
    /// </code>
    ///     <para>
    ///         Finally you can resolve them either by using dependency injection or by service location:
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// var fetcher = container.Resolve<IMyDataFetcher>();
    /// var cache = container.Resolve<UserCache>();
    /// ]]>
    /// </code>
    /// </remarks>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Register a specific service
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="concrete">The class that implements one or more services.</param>
        /// <remarks>
        ///     <para>Requires that the type has been tagged with the <c>[ContainerService]</c> attribute</para>
        ///     This method will check which interfaces the type implements. It will registered the type as any non <c>System.*</c>
        ///     interfaces. If the class do not implement any interfaces it will be registered as itself.
        /// </remarks>
        public static void RegisterService(this IServiceCollection builder, Type concrete)
        {
            var attr =
                concrete.GetCustomAttributes(typeof(ContainerServiceAttribute), false)
                    .Cast<ContainerServiceAttribute>()
                    .FirstOrDefault();
            if (attr == null)
                throw new InvalidOperationException("Missing the [ContainerService] attribute on '" + concrete.FullName +
                                                    "', can therefore not know which lifetime to use.");

            RegisterService(builder, concrete, attr.Lifetime);
        }

        /// <summary>
        ///     Register a specific service
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="concrete">The class that implements one or more services.</param>
        /// <param name="lifetime">Lifetime.</param>
        /// <remarks>
        ///     This method will check which interfaces the type implements. It will registered the type as any non <c>System.*</c>
        ///     interfaces. If the class do not implement any interfaces it will be registered as itself.
        /// </remarks>
        public static void RegisterService(this IServiceCollection builder, Type concrete, ContainerLifetime lifetime)
        {
            var interfaces = concrete.GetInterfaces().Where(x => !x.Namespace.StartsWith("System"));
            if (interfaces.Any())
            {
                foreach (var @interface in interfaces)
                {
                    switch (lifetime)
                    {
                        case ContainerLifetime.Scoped:
                            builder.AddScoped(@interface, concrete);
                            break;
                        case ContainerLifetime.SingleInstance:
                            builder.AddSingleton(@interface, concrete);
                            break;
                        case ContainerLifetime.Transient:
                            builder.AddTransient(@interface, concrete);
                            break;
                    } 
                }

                return;
            }


            switch (lifetime)
            {
                case ContainerLifetime.Scoped:
                    builder.AddScoped(concrete);
                    break;
                case ContainerLifetime.SingleInstance:
                    builder.AddSingleton(concrete);
                    break;
                case ContainerLifetime.Transient:
                    builder.AddTransient(concrete);
                    break;
            }
        }
        
        /// <summary>
        ///     Will search for the <see cref="ContainerServiceAttribute" /> and register all classes that is tagged with it.
        /// </summary>
        /// <param name="builder">the ServiceCollection builder</param>
        /// <param name="assemblies">Specified assemblies</param>
        /// <remarks>
        /// </remarks>
        public static void RegisterServices(this IServiceCollection builder, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr =
                        type.GetCustomAttributes(typeof(ContainerServiceAttribute), false)
                            .Cast<ContainerServiceAttribute>()
                            .FirstOrDefault();
                    if (attr == null)
                        continue;

                    RegisterService(builder, type, attr.Lifetime);
                }
            }
        }
    }
}
