using System.Reflection;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs.Demo
{
    public class SimpleCqsBuilder
    {
        private readonly SimpleCommandBus _cmdBus;
        private readonly SimpleEventBus _eventBus;
        private readonly SimpleQueryBus _queryBus;
        private readonly SimpleRequestReplyBus _requestReplyBus;

        public SimpleCqsBuilder()
        {
            _cmdBus = new SimpleCommandBus();
            _queryBus = new SimpleQueryBus();
            _requestReplyBus = new SimpleRequestReplyBus();
            _eventBus = new SimpleEventBus();
            CqsBus.CmdBus = _cmdBus;
            CqsBus.EventBus = _eventBus;
            CqsBus.QueryBus = _queryBus;
            CqsBus.RequestReplyBus = _requestReplyBus;
        }

        public void Register(Assembly assembly)
        {
            _cmdBus.Register(assembly);
            _queryBus.Register(assembly);
            _requestReplyBus.Register(assembly);
            _eventBus.Register(assembly);
        }
    }
}