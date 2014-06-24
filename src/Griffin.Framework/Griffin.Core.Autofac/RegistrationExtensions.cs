using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Griffin.Container;

namespace Griffin.Core.Autofac
{
    /// <summary>
    ///     Extension methods for autofac.
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
    ///         Next you register them in autofac:
    ///     </para>
    ///     <code>
    /// 
    /// var cb = new ContainerBuilder();
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
    public static class ContainerBuilderExtensions
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
        public static void RegisterService(this ContainerBuilder builder, Type concrete)
        {
            var attr =
                concrete.GetCustomAttributes(typeof (ContainerServiceAttribute), false)
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
        public static void RegisterService(this ContainerBuilder builder, Type concrete, ContainerLifetime lifetime)
        {
            var interfaces = concrete.GetInterfaces().Where(x => !x.Namespace.StartsWith("System"));
            if (interfaces.Any())
            {
                switch (lifetime)
                {
                    case ContainerLifetime.Scoped:
                        builder.RegisterType(concrete).AsImplementedInterfaces().AsSelf().OwnedByLifetimeScope();
                        break;
                    case ContainerLifetime.SingleInstance:
                        builder.RegisterType(concrete).AsImplementedInterfaces().AsSelf().SingleInstance();
                        break;
                    case ContainerLifetime.Transient:
                        builder.RegisterType(concrete).AsImplementedInterfaces().AsSelf();
                        break;
                }

                return;
            }


            switch (lifetime)
            {
                case ContainerLifetime.Scoped:
                    builder.RegisterType(concrete).AsSelf().InstancePerLifetimeScope();
                    break;
                case ContainerLifetime.SingleInstance:
                    builder.RegisterType(concrete).AsSelf().SingleInstance();
                    break;
                case ContainerLifetime.Transient:
                    builder.RegisterType(concrete).AsSelf();
                    break;
            }
        }

        /// <summary>
        ///     Will search for the <see cref="ContainerServiceAttribute" /> and register all classes that is tagged with it.
        /// </summary>
        /// <param name="builder">the Autofac builder</param>
        /// <param name="assemblies">Specified assemblies</param>
        /// <remarks>
        /// </remarks>
        public static void RegisterServices(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr =
                        type.GetCustomAttributes(typeof (ContainerServiceAttribute), false)
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