using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Authentication.Messages;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
using Griffin.Security;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Client used to talk with the server defined in the <c>Griffin.Cqs.Server</c> nuget package.
    /// </summary>
    public class CqsClient : ICommandBus, IEventBus, IQueryBus, IRequestReplyBus, IDisposable
    {
        private readonly Timer _cleanuptimer;
        private readonly ChannelTcpClient _client;
        private readonly ConcurrentDictionary<Guid, Waiter> _response = new ConcurrentDictionary<Guid, Waiter>();
        private IPEndPoint _endPoint;
        private NetworkCredential _credentials;
        private IPasswordHasher _hasher;
        private IAuthenticationMessageFactory _authenticationMessageFactory = new AuthenticationMessageFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="CqsClient"/> class.
        /// </summary>
        public CqsClient()
        {
            _client = new ChannelTcpClient(
                new MicroMessageEncoder(new DataContractMessageSerializer()),
                new MicroMessageDecoder(new DataContractMessageSerializer()));
            _client.Filter = OnMessageReceived;
            _cleanuptimer = new Timer(OnCleanup, 0, 10000, 10000);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CqsClient"/> class.
        /// </summary>
        /// <param name="serializer">Serializer to be used for the transported messages.</param>
        public CqsClient(Func<IMessageSerializer> serializer)
        {
            _client = new ChannelTcpClient(new MicroMessageEncoder(serializer()),
                new MicroMessageDecoder(serializer()));
            _client.Filter = OnMessageReceived;
            _cleanuptimer = new Timer(OnCleanup);
        }

        /// <summary>
        ///     Assign if you want to use secure communication.
        /// </summary>
        public ISslStreamBuilder Certificate
        {
            get { return _client.Certificate; }
            set { _client.Certificate = value; }
        }

        /// <summary>
        /// Used to build messages that are being sent over the network for authentication.
        /// </summary>
        /// <seealso cref="EnableAuthentication"/>
        public IAuthenticationMessageFactory AuthenticationMessageFactory
        {
            get { return _authenticationMessageFactory; }
            set { _authenticationMessageFactory = value; }
        }

        /// <summary>
        /// Use this method if you want to use the built in authentication library.
        /// </summary>
        /// <param name="credential">Credential used by the user to authenticate.</param>
        /// <remarks>
        /// <para>
        /// To make this work you need to have stored passwords in your database by hashing them with a salt. The hash must have been
        /// created using <c>Rfc2898DeriveBytes</c> with 1000 iterations upon the string "password:hash". You you are using another 
        /// way of storing passwords in your DB you need to configure the authentication process using <see cref="ConfigureAuthentication"/>.
        /// </para>
        /// </remarks>
        public NetworkCredential Credential
        {
            get { return _credentials; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _credentials = value;
            }
        }

        /// <summary>
        /// Used to configure how passwords are being hashed and which messages should be used when transfering authentication information
        /// over the network.
        /// </summary>
        /// <param name="hasher">Password hasher, the dault one is <see cref="PasswordHasherRfc2898"/>.</param>
        /// <param name="messageFactory">Message factory, the default on is <see cref="AuthenticationMessageFactory"/>.</param>
        public void ConfigureAuthentication(IPasswordHasher hasher, IAuthenticationMessageFactory messageFactory)
        {
            if (hasher == null) throw new ArgumentNullException("hasher");
            if (messageFactory == null) throw new ArgumentNullException("messageFactory");
            _authenticationMessageFactory = messageFactory;
            _hasher = hasher;
        }


        /// <summary>
        /// Execute a command and wait for result (i.e. exception for failure or just return for success)
        /// </summary>
        /// <typeparam name="T">Type of command</typeparam>
        /// <param name="command">command object</param>
        /// <returns>completion task</returns>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            await EnsureConnected();
            var waiter = new Waiter<T>(command.CommandId);
            _response[command.CommandId] = waiter;
            await _client.SendAsync(command);
            await waiter.Task;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cleanuptimer.Dispose();
        }

        /// <summary>
        /// Publishes an application event (at server side)
        /// </summary>
        /// <typeparam name="TApplicationEvent">The type of the application event.</typeparam>
        /// <param name="e">event to publish.</param>
        /// <returns>completion task</returns>
        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e)
            where TApplicationEvent : ApplicationEvent
        {
            await EnsureConnected();
            var waiter = new Waiter<TApplicationEvent>(e.EventId);
            _response[e.EventId] = waiter;
            await _client.SendAsync(e);
            await waiter.Task;
        }

        /// <summary>
        /// Queries the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">query to get a response for.</param>
        /// <returns>completion task</returns>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            await EnsureConnected();
            var waiter = new Waiter<TResult>(query.QueryId);
            _response[query.QueryId] = waiter;
            await _client.SendAsync(query);
            await waiter.Task;
            return ((dynamic)waiter.Task).Result;
        }

        /// <summary>
        /// Execute a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">The type of the reply.</typeparam>
        /// <param name="request">Request to get a reply for.</param>
        /// <returns>completion task</returns>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            await EnsureConnected();
            var waiter = new Waiter<TReply>(request.RequestId);
            _response[request.RequestId] = waiter;
            await _client.SendAsync(request);
            await waiter.Task;
            return ((dynamic)waiter.Task).Result;
        }

        /// <summary>
        /// Start client (will autoconnect if getting disconnected)
        /// </summary>
        /// <param name="address">The address for the CQS server.</param>
        /// <param name="port">The port that the CQS server is listening on.</param>
        /// <returns></returns>
        public async Task StartAsync(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
            if (_credentials != null)
            {
                await Authenticate();
            }
        }

        private async Task EnsureConnected()
        {
            if (!_client.IsConnected)
            {
                if (_endPoint == null)
                    throw new InvalidOperationException("Call 'Start()' first.");

                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
                if (_credentials != null)
                {
                    await Authenticate();
                }
            }
        }

        private async Task Authenticate()
        {
            var handshake = _authenticationMessageFactory.CreateHandshake(_credentials.UserName);
            await _client.SendAsync(handshake);
            var handshakeReply = await _client.ReceiveAsync<AuthenticationHandshakeReply>();
            var passwordHash = _hasher.HashPassword(_credentials.Password, handshakeReply.AccountSalt);
            var token = _hasher.HashPassword(passwordHash, handshakeReply.SessionSalt);
            var auth = _authenticationMessageFactory.CreateAuthentication(token);
            var clientSalt = auth.ClientSalt;
            await _client.SendAsync(auth);
            var result = await _client.ReceiveAsync<AuthenticateReply>();
            if (result.State == AuthenticateReplyState.Success)
            {
                var ourToken = _hasher.HashPassword(passwordHash, clientSalt);
                if (!_hasher.Compare(ourToken, result.AuthenticationToken))
                    throw new AuthenticationException(
                        "Server failed to prove it's identity. The hashed token do not match ours.");
            }
            else
                throw new AuthenticationException("We failed to authenticate with the server. Result: " + result.State);
        }

        private void OnCleanup(object state)
        {
            try
            {
                var values = _response.Values;
                foreach (var waiter in values)
                {
                    Waiter removed;
                    if (waiter.Expired)
                        _response.TryRemove(waiter.Id, out removed);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }
        }

        /// <summary>
        ///     Always revoke since we do not want incoming messages to be queued up for receive operations.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ClientFilterResult OnMessageReceived(ITcpChannel channel, object message)
        {
            //currently authentication etc are not wrapped, so we need to do it like this.
            var result = message as ClientResponse;
            if (result == null)
            {
                Waiter waiter;
                if (_response.Count != 1)
                    throw new InvalidOperationException(
                        "More than one pending message and we received an unwrapped object: " + message);

                var key = _response.Keys.First();
                if (!_response.TryRemove(key, out waiter))
                {
                    //TODO: LOG
                    return ClientFilterResult.Revoke;
                }
                waiter.Trigger(message);
            }

            else
            {
                Waiter waiter;
                if (!_response.TryRemove(result.Identifier, out waiter))
                {
                    //TODO: LOG
                    return ClientFilterResult.Revoke;
                }
                waiter.Trigger(result.Body);
            }



            return ClientFilterResult.Revoke;
        }

        private abstract class Waiter
        {
            protected Waiter(Guid id)
            {
                Id = id;
                WaitedSince = DateTime.UtcNow;
            }

            public Guid Id { get; set; }

            private DateTime WaitedSince { get; set; }
            public abstract Task Task { get; }

            public bool Expired
            {
                get { return DateTime.UtcNow.Subtract(WaitedSince).TotalSeconds > 10; }
            }

            public abstract void Trigger(object result);
        }

        private class Waiter<T> : Waiter
        {
            private readonly TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();

            public Waiter(Guid id)
                : base(id)
            {
            }

            public override Task Task
            {
                get { return _completionSource.Task; }
            }

            public override void Trigger(object result)
            {
                if (result is Exception)
                {
                    if (result is AggregateException)
                        _completionSource.SetException(new ServerSideException("Server failed to execute.",
                            ((AggregateException)result).InnerException));
                    else
                        _completionSource.SetException((Exception)result);
                }

                else
                    _completionSource.SetResult((T)result);
            }
        }
    }
}