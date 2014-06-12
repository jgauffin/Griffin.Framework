using System.Reflection;
using Autofac;
using DotNetCqs;
using Griffin.Core.Autofac;
using Griffin.Core.Autofac.Cqs;
using Griffin.Cqs.InversionOfControl;

namespace Griffin.Cqs.Demo
{
    public class CompositionRoot
    {
        private AutofacAdapter _adapter;

        public void Build()
        {
            var cb = new ContainerBuilder();

            // registers all our classes.
            cb.RegisterCqsHandlers(Assembly.GetExecutingAssembly());

            cb.RegisterType<IocCommandBus>().AsImplementedInterfaces().SingleInstance();
            cb.RegisterType<IocEventBus>().AsImplementedInterfaces().SingleInstance();
            cb.RegisterType<IocQueryBus>().AsImplementedInterfaces().SingleInstance();
            cb.RegisterType<IocRequestReplyBus>().AsImplementedInterfaces().SingleInstance();
            cb.Register(x => _adapter).AsImplementedInterfaces().SingleInstance();

            _adapter = new AutofacAdapter(cb.Build());

            CqsBus.CmdBus = _adapter.Resolve<ICommandBus>();
            CqsBus.QueryBus = _adapter.Resolve<IQueryBus>();
            CqsBus.RequestReplyBus = _adapter.Resolve<IRequestReplyBus>();
            CqsBus.EventBus = _adapter.Resolve<IEventBus>();
        }
    }
}