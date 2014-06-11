using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Griffin.Core.Autofac
{
    /// <summary>
    /// Extension methods for different parts of the Griffin Library
    /// </summary>
    public static class ContainerBuilderExtensions
    {

        /// <summary>
        /// Register all different types of CQS handlers (for commands, queries, events and request/reply).
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        /// <para>
        /// Will search for all classes that implement one of our handler interfaces defined in the <c>DotNetCqs</c> library.
        /// </para>
        /// </remarks>
        public static void RegisterCqsHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            RegisterQueryHandlers(builder);
            RegisterCommandHandlers(builder);
            RegisterApplicationEventHandlers(builder);
            RegisterRequestReplyHandlers(builder);
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterQueryHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsQueryHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterCommandHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsCommandHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterApplicationEventHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsEventHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRequestReplyHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsRequestReplyHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        private static bool IsQueryHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IApplicationEventSubscriber" && x.Namespace == "DotNetCqs");
        }

        private static bool IsCommandHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "ICommandHandler" && x.Namespace == "DotNetCqs");
        }

        private static bool IsEventHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IQueryHandler" && x.Namespace == "DotNetCqs");
        }

        private static bool IsRequestReplyHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IRequestHandler" && x.Namespace == "DotNetCqs");
        }

        
    }
}
