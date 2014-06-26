using System;
using System.Net;
using System.Threading.Tasks;
using Griffin.Net.Channels;
using Griffin.Net.LiteServer.Modules;
using Griffin.Net.Protocols;

namespace Griffin.Net.LiteServer
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
            _listener.ClientConnected += OnClientConnect;
            _listener.ClientDisconnected += OnClientDisconnect;
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

        private async Task ExecuteConnectModules(ITcpChannel channel, ClientContext context)
        {
            var result = ModuleResult.Continue;

            for (var i = 0; i < _modules.Length; i++)
            {
                var connectMod = _modules[i] as IConnectionEvents;
                if (connectMod == null)
                    continue;

                try
                {
                    result = await connectMod.OnClientConnected(context);
                }
                catch (Exception exception)
                {
                    context.Error = exception;
                    result = ModuleResult.SendResponse;
                }

                if (result != ModuleResult.Continue)
                    break;
            }

            switch (result)
            {
                case ModuleResult.Disconnect:
                    channel.Close();
                    break;
                case ModuleResult.SendResponse:
                    channel.Send(context.ResponseMessage);
                    break;
            }
        }

        private async Task ExecuteDisconnectModules(ITcpChannel channel, ClientContext context)
        {
            for (var i = 0; i < _modules.Length; i++)
            {
                var connectMod = _modules[i] as IConnectionEvents;
                if (connectMod == null)
                    continue;

                try
                {
                    await connectMod.OnClientDisconnect(context);
                }
                catch (Exception exception)
                {
                    //TODO: Alert user of failure
                }
            }
        }

        private async Task ExecuteModules(ITcpChannel channel, ClientContext context)
        {
            var result = ModuleResult.Continue;

            for (var i = 0; i < _modules.Length; i++)
            {
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


            try
            {
                await EndRequestAsync(context);
            }
            catch (Exception exception)
            {
                if (context.ResponseMessage == null)
                    context.ResponseMessage = exception;
                result = ModuleResult.Disconnect;
            }

            if (context.ResponseMessage != null)
                channel.Send(context.ResponseMessage);

            if (result == ModuleResult.Disconnect)
            {
                channel.Close();
            }
        }

        private void OnClientConnect(object sender, ClientConnectedEventArgs e)
        {
            var context = new ClientContext(e.Channel, null);
            ExecuteConnectModules(e.Channel, context).Wait();
        }

        private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
        {
            var context = new ClientContext(e.Channel, null);
            ExecuteDisconnectModules(e.Channel, context).Wait();
        }

        private void OnClientMessage(ITcpChannel channel, object message)
        {
            var context = new ClientContext(channel, message);
            ExecuteModules(channel, context).Wait();
        }
    }
}