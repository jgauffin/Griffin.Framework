using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;

namespace Griffin.Core.Unity.Cqs
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
        public static void RegisterApplicationEventHandlers(this IUnityContainer container, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsEventHandler))
            {
                container.Register(type, new HierarchicalLifetimeManager());
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
        public static void RegisterCommandHandlers(this IUnityContainer container, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsCommandHandler))
            {
                container.Register(type, new HierarchicalLifetimeManager());
            }
        }

        /// <summary>
        ///     Register all different types of CQS handlers (for commands, queries, events and request/reply).
        /// </summary>
        /// <param name="container">instance</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <remarks>
        ///     <para>
        ///         Will search for all classes that implement one of our handler interfaces defined in the <c>DotNetCqs</c>
        ///         library.
        ///     </para>
        /// </remarks>
        public static void RegisterCqsHandlers(this IUnityContainer container, params Assembly[] assemblies)
        {
            if (container == null) throw new ArgumentNullException("container");
            RegisterQueryHandlers(container, assemblies);
            RegisterCommandHandlers(container, assemblies);
            RegisterApplicationEventHandlers(container, assemblies);
            RegisterRequestReplyHandlers(container, assemblies);
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
        public static void RegisterQueryHandlers(this IUnityContainer container, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsQueryHandler))
            {
                container.Register(type, new HierarchicalLifetimeManager());
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
        public static void RegisterRequestReplyHandlers(this IUnityContainer container, params Assembly[] assemblies)
        {
            foreach (var type in ForTypes(IsRequestReplyHandler))
            {
                container.Register(type, new HierarchicalLifetimeManager());
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