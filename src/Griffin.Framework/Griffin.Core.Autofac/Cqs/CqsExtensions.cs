using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Griffin.Core.Autofac.Cqs
{
    /// <summary>
    ///     Extension methods for the CQS implementation in Griffin Framework.
    /// </summary>
    public static class CqsExtensions
    {
        /// <summary>
        ///     Register all different types of CQS handlers (for commands, queries, events and request/reply).
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one of our handler interfaces defined in the <c>DotNetCqs</c>
        ///         library.
        ///     </para>
        /// </remarks>
        public static void RegisterCqsHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            RegisterQueryHandlers(builder, assemblies);
            RegisterCommandHandlers(builder, assemblies);
            RegisterApplicationEventHandlers(builder, assemblies);
            RegisterRequestReplyHandlers(builder, assemblies);
        }

        /// <summary>
        ///     Register all query handlers
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IQueryHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterQueryHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsQueryHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        /// <summary>
        ///     Register all command handlers .
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>ICommandHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterCommandHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsCommandHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        /// <summary>
        ///     Register all application event handlers
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IApplicationEventHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterApplicationEventHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return
                builder.RegisterAssemblyTypes(assemblies)
                    .Where(IsEventHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        /// <summary>
        ///     Register all request/reply handlers
        /// </summary>
        /// <param name="builder">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IRequestReply</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterRequestReplyHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
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
            return arg.GetInterfaces().Any(x => x.Name == "IApplicationEventSubscriber`1" && x.Namespace == "DotNetCqs");
        }

        private static bool IsCommandHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "ICommandHandler`1" && x.Namespace == "DotNetCqs");
        }

        private static bool IsEventHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IQueryHandler`2" && x.Namespace == "DotNetCqs");
        }

        private static bool IsRequestReplyHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IRequestHandler`2" && x.Namespace == "DotNetCqs");
        }
    }
}