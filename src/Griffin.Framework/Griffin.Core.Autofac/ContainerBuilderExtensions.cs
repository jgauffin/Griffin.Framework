using System;
using System.Linq;
using System.Reflection;

namespace Griffin.Cqs
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterQueryHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsQueryHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterCommandHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsCommandHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterApplicationEventHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsEventHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRequestReplyHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
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
