using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Core.Microsoft.Extensions.DependencyInjection.Cqs
{
    /// <summary>
    ///     Extension methods for the CQS implementation in Griffin Framework.
    /// </summary>
    public static class CqsExtensions
    {
        /// <summary>
        ///     Register all application event handlers
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IApplicationEventHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static void RegisterApplicationEventHandlers(this IServiceCollection builder, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsEventHandler))
            {
                builder.RegisterService(type, Container.ContainerLifetime.Scoped);
            }
        }

        /// <summary>
        ///     Register all command handlers .
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>ICommandHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static void RegisterCommandHandlers(this IServiceCollection builder, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsCommandHandler))
            {
                builder.RegisterService(type, Container.ContainerLifetime.Scoped);
            }
        }

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
        public static void RegisterCqsHandlers(this IServiceCollection builder, params Assembly[] assemblies)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            RegisterQueryHandlers(builder, assemblies);
            RegisterCommandHandlers(builder, assemblies);
            RegisterApplicationEventHandlers(builder, assemblies);
            RegisterRequestReplyHandlers(builder, assemblies);
        }

        /// <summary>
        ///     Register all query handlers
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IQueryHandler</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static void RegisterQueryHandlers(this IServiceCollection builder, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsQueryHandler))
            {
                builder.RegisterService(type, Container.ContainerLifetime.Scoped);
            }
        }

        /// <summary>
        ///     Register all request/reply handlers
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one <c>IRequestReply</c> interface that is defined in the
        ///         <c>DotNetCqs</c> library.
        ///     </para>
        /// </remarks>
        public static void RegisterRequestReplyHandlers(this IServiceCollection builder, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsRequestReplyHandler))
            {
                builder.RegisterService(type, Container.ContainerLifetime.Scoped);
            }
        }

        private static IEnumerable<Type> ForTypes(Func<Type, bool> filter, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    if (filter(type))
                        yield return type;
                }
            }
        }

        private static bool IsCommandHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "ICommandHandler`1" && x.Namespace == "DotNetCqs");
        }

        private static bool IsEventHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IQueryHandler`2" && x.Namespace == "DotNetCqs");
        }

        private static bool IsQueryHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IApplicationEventSubscriber`1" && x.Namespace == "DotNetCqs");
        }

        private static bool IsRequestReplyHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.Name == "IRequestHandler`2" && x.Namespace == "DotNetCqs");
        }
    }
}
