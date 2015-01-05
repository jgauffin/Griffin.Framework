using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using DisconnectHandler = Griffin.Net.Protocols.Stomp.Broker.MessageHandlers.DisconnectHandler;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    ///     Broker that allows end points to subscribing on queues to deliver and receive messages.
    /// </summary>
    public class StompBroker
    {
        private readonly List<StompClient> _clients = new List<StompClient>();
        private readonly Dictionary<string, IFrameHandler> _frameHandlers = new Dictionary<string, IFrameHandler>();
        private readonly StompTcpListener _tcpListener;
        private IAuthenticationService _authenticationService;
        private IQueueRepository _queues;


        /// <summary>
        ///     Initializes a new instance of the <see cref="StompBroker" /> class.
        /// </summary>
        /// <param name="repository">
        ///     Used to provide all queues that this broker is for. There is a built in class,
        ///     <see cref="MemoryQueueRepository" />, which you can use.
        /// </param>
        /// <exception cref="System.ArgumentNullException">repository</exception>
        public StompBroker(IQueueRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            _queues = repository;
            _tcpListener = new StompTcpListener {MessageReceived = OnMessageReceived, MessageSent = OnMessageDelivered};
            _tcpListener.ClientConnected += OnClientConnected;
            _tcpListener.ClientDisconnected += OnClientDisconnected;

            ServerName = "Griffin.Queue/1.0";

            var connectHandler = new ConnectHandler(new NoAuthenticationService(), ServerName);
            _frameHandlers.Add("ACK", new AckHandler());
            _frameHandlers.Add("ABORT", new AbortHandler());
            _frameHandlers.Add("BEGIN", new BeginHandler());
            _frameHandlers.Add("COMMIT", new CommitHandler());
            _frameHandlers.Add("CONNECT", connectHandler);
            _frameHandlers.Add("DISCONNECT", new DisconnectHandler());
            _frameHandlers.Add("NACK", new NackHandler(_queues));
            _frameHandlers.Add("SEND", new SendHandler(_queues));
            _frameHandlers.Add("SUBSCRIBE", new SubscribeHandler(_queues));
            _frameHandlers.Add("STOMP", connectHandler);
        }

        /// <summary>
        ///     Service used to authenticate incoming connections.
        /// </summary>
        public IAuthenticationService AuthenticationService
        {
            get { return _authenticationService; }
            set
            {
                _authenticationService = value;
                _frameHandlers["CONNECT"] = new ConnectHandler(value, ServerName);
                _frameHandlers["STOMP"] = new ConnectHandler(value, ServerName);
            }
        }

        /// <summary>
        ///     Name of the server.
        /// </summary>
        /// <value>
        ///     Should be in the format "ServerName/versionNumber".
        /// </value>
        /// <example>
        ///     Griffin Queue/1.0
        /// </example>
        public string ServerName { get; set; }

        /// <summary>
        ///     Port that the server is listening on.
        /// </summary>
        /// <remarks>
        ///     You can use port <c>0</c> in <see cref="Start" /> to let the OS assign a port. This method will then give you the
        ///     assigned port.
        /// </remarks>
        public int LocalPort
        {
            get { return _tcpListener.LocalPort; }
        }

        //TODO: Use a dictionary instead.
        private StompClient GetClient(ITcpChannel channel)
        {
            return _clients.First(x => x.IsForChannel(channel));
        }

        /// <summary>
        ///     Start broker to be able to receive incoming connections.
        /// </summary>
        /// <param name="address">Address to bind to</param>
        /// <param name="port">Port to accept connections on.</param>
        public void Start(IPAddress address, int port)
        {
            _tcpListener.Start(address, port);
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            _clients.Add(new StompClient(e.Channel, new TransactionManager()));
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            var client = GetClient(e.Channel);
            if (client != null)
                client.Cleanup();

            //TODO: Log if we can't match the client.
        }

        private void OnMessageDelivered(ITcpChannel channel, object message)
        {
        }

        private void OnMessageReceived(ITcpChannel channel, object message)
        {
            var client = GetClient(channel);
            var frame = (IFrame) message;

            if (!client.IsAuthenticated)
            {
                if (frame.Name != "STOMP" && frame.Name != "CONNECT")
                {
                    var error = frame.CreateError("You must send the 'STOMP' frame first.");
                    client.Send(error);
                    return;
                }
            }

            var name = frame.Name.ToUpper();
            IFrameHandler handler;
            if (!_frameHandlers.TryGetValue(name, out handler))
            {
                var error = frame.CreateError("The frame '" + name + "' is unknown for this server.");
                client.Send(error);
                return;
            }

            try
            {
                var response = handler.Process(client, frame);
                if (response != null)
                    client.Send(frame);
            }
            catch (BadRequestException exception)
            {
                var error = exception.Request.CreateError(exception.Message);
                client.Send(error);
            }
        }
    }
}