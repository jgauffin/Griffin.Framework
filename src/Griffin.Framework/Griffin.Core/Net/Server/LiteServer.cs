using System;
using System.Net;
using System.Threading.Tasks;
using Griffin.Cqs.Net.Modules;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Serializers;
using Griffin.Net.Server.Modules;

namespace Griffin.Net.Server
{
    /// <summary>
    ///     Lightweight server.
    /// </summary>
    public class LiteServer
    {
        private readonly ChannelTcpListener _listener;
        private readonly IServerModule[] _modules;


        /// <summary>
        ///     Initializes a new instance of the <see cref="LiteServer" /> class.
        /// </summary>
        public LiteServer(LiteServerConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _modules = configuration.Modules.Build();
            var config = new ChannelTcpListenerConfiguration(configuration.DecoderFactory, configuration.EncoderFactory);
            _listener = new ChannelTcpListener(config);
            if (configuration.Certificate != null)
                _listener.ChannelFactory =
                    new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(configuration.Certificate));

            _listener.MessageReceived = OnClientMessage;
        }

        /// <summary>
        ///     Port that we got assigned (or specified)
        /// </summary>
        public int LocalPort
        {
            get { return _listener.LocalPort; }
        }


        public void Start(IPAddress address, int port)
        {
            _listener.Start(address, port);
        }

        private async Task EndRequestAsync(ClientContext context)
        {
            for (var i = 0; i < _modules.Length; i++)
            {
                await _modules[i].EndRequest(context);
            }
        }

        private async Task ExecuteModules(ITcpChannel channel, ClientContext context)
        {
            var result = ModuleResult.Continue;

            for (var i = 0; i < _modules.Length; i++)
            {
                var failed = false;
                try
                {
                    await _modules[i].BeginRequestAsync(context);
                }
                catch (Exception exception)
                {
                    context.Error = exception;
                    result = ModuleResult.SendResponse;
                }
            }


            if (result == ModuleResult.Continue)
            {
                for (var i = 0; i < _modules.Length; i++)
                {
                    try
                    {
                        result = await _modules[i].ProcessAsync(context);
                    }
                    catch (Exception exception)
                    {
                        context.Error = exception;
                        result = ModuleResult.SendResponse;
                    }

                    if (result != ModuleResult.Continue)
                        break;
                }
            }


            await EndRequestAsync(context);
            if (context.ResponseMessage != null)
                channel.Send(context.ResponseMessage);

            if (result == ModuleResult.Disconnect)
            {
                channel.Close();
            }
        }

        private void OnClientMessage(ITcpChannel channel, object message)
        {
            var context = new ClientContext(channel, message);
            ExecuteModules(channel, context);
        }
    }
}