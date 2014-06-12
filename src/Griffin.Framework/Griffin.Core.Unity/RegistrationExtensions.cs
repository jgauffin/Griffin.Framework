using System;
using System.Linq;
using System.Reflection;
using Griffin.Container;
using Microsoft.Practices.Unity;

namespace Griffin.Core.Unity
{
    /// <summary>
    ///     Extension methods for simplified registrations
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
    /// var container = new UnityContainer();
    /// container.RegisterServices(Assembly.GetExecutingAssembly());
    /// 
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
    public static class RegistrationExtensions
    {
        /// <summary>
        ///     Register a type as all implemented interface (except those that start with <c>System</c>).
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="concrete">Class to register</param>
        /// <param name="lifetime">
        ///     The lifetime. Typically <c>HierarchicalLifetimeManager</c> (scoped),
        ///     <c>ContainerControlledLifetimeManager</c> (singleton) or <c>TransientLifetimeManager</c> (transient)
        /// </param>
        public static void Register(this IUnityContainer container, Type concrete, LifetimeManager lifetime)
        {
            // required so that all interface registrations uses the same instance.
            container.RegisterType(concrete, concrete, lifetime);

            var interfaces = concrete.GetInterfaces().Where(x => !x.Namespace.StartsWith("System"));
            foreach (var @interface in interfaces)
            {
                container.RegisterType(@interface, concrete, lifetime);
            }
        }

        /// <summary>
        ///     Register a specific service
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="concrete">The class that implements one or more services.</param>
        /// <remarks>
        ///     <para>Requires that the type has been tagged with the <c>[ContainerService]</c> attribute</para>
        ///     This method will check which interfaces the type implements. It will registered the type as any non <c>System.*</c>
        ///     interfaces. If the class do not implement any interfaces it will be registered as itself.
        /// </remarks>
        public static void RegisterService(this IUnityContainer container, Type concrete)
        {
            var attr =
                concrete.GetCustomAttributes(typeof (ContainerServiceAttribute), false)
                    .Cast<ContainerServiceAttribute>()
                    .FirstOrDefault();
            if (attr == null)
                throw new InvalidOperationException("Missing the [ContainerService] attribute on '" + concrete.FullName +
                                                    "', can therefore not know which lifetime to use.");

            RegisterService(container, concrete, attr.Lifetime);
        }

        /// <summary>
        ///     Register a specific service
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="concrete">The class that implements one or more services.</param>
        /// <param name="lifetime">Lifetime.</param>
        /// <remarks>
        ///     This method will check which interfaces the type implements. It will registered the type as any non <c>System.*</c>
        ///     interfaces. If the class do not implement any interfaces it will be registered as itself.
        /// </remarks>
        public static void RegisterService(this IUnityContainer container, Type concrete, ContainerLifetime lifetime)
        {
            switch (lifetime)
            {
                case ContainerLifetime.Scoped:
                    Register(container, concrete, new HierarchicalLifetimeManager());
                    break;
                case ContainerLifetime.SingleInstance:
                    Register(container, concrete, new ContainerControlledLifetimeManager());
                    break;
                case ContainerLifetime.Transient:
                    Register(container, concrete, new TransientLifetimeManager());
                    break;
            }
        }

        /// <summary>
        ///     Will search for the <see cref="ContainerServiceAttribute" /> and register all classes that is tagged with it.
        /// </summary>
        /// <param name="container">the Autofac container</param>
        /// <param name="assemblies">Specified assemblies</param>
        /// <remarks>
        /// </remarks>
        public static void RegisterServices(this IUnityContainer container, params Assembly[] assemblies)
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

                    RegisterService(container, type, attr.Lifetime);
                }
            }
        }
    }
}