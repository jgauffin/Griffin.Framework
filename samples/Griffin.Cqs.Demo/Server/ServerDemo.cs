using System.Net;
using Griffin.Core.Json;
using Griffin.Cqs.Net;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Server;

namespace Griffin.Cqs.Demo.Server
{
    public class ServerDemo
    {
        private LiteServer _server;

        public int LocalPort
        {
            get { return _server.LocalPort; }
        }

        public void Setup()
        {
            var root = new CompositionRoot();
            root.Build();

            var module = new CqsModule
            {
                CommandBus = CqsBus.CmdBus,
                QueryBus = CqsBus.QueryBus,
                RequestReplyBus = CqsBus.RequestReplyBus,
                EventBus = CqsBus.EventBus
            };


            var config = new LiteServerConfiguration();
            config.DecoderFactory = () => new MicroMessageDecoder(new JsonMessageSerializer());
            config.EncoderFactory = () => new MicroMessageEncoder(new JsonMessageSerializer());

            config.Modules.AddAuthentication(new AuthenticationModule());
            config.Modules.AddHandler(module);

            _server = new LiteServer(config);
        }

        public void Start()
        {
            _server.Start(IPAddress.Any, 0);
        }
    }
}