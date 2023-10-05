using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Authentication;
using Griffin.Net.Authentication.Messages;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
using Griffin.Security;

namespace Griffin.Cqs.Net
{
    /// <summary>
    ///     Client used to talk with the server defined in the <c>Griffin.Cqs.Server</c> nuget package.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Uses TCP and the MicroMsg protocol to transport messages to server side.
    ///     </para>
    /// </remarks>
    public class CqsClient : IMessageBus, IQueryBus, IDisposable
    {
        private readonly Timer _cleanuptimer;
        private readonly ChannelTcpClient _client;
        private readonly ConcurrentDictionary<Guid, Waiter> _response = new ConcurrentDictionary<Guid, Waiter>();
        private bool _continueAuthenticate;
        private IPEndPoint _endPoint;
        private object _lastSentItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
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
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
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
            get => _client.Certificate;
            set => _client.Certificate = value;
        }

        /// <summary>
        ///     Set if you want to authenticate against a server.
        /// </summary>
        public IClientAuthenticator Authenticator
        {
            get => _client.Authenticator;
            set => _client.Authenticator = value;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cleanuptimer.Dispose();
        }
        
        public async Task<TResult> QueryAsync<TResult>(ClaimsPrincipal principal, Query<TResult> query)
        {
            var msg = new Message(query);
            msg.Properties["MessageType"] = "QUERY";
            //todo: Add JWT token

            await EnsureConnected();
            var waiter = new Waiter<TResult>(msg.MessageId);
            _response[msg.MessageId] = waiter;
            await SendItem(query);
            await waiter.Task;
            return ((dynamic)waiter.Task).Result;
        }

        /// <summary>
        ///     Queries the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">query to get a response for.</param>
        /// <returns>completion task</returns>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            var msg = new Message(query);
            msg.Properties["MessageType"] = "QUERY";

            await EnsureConnected();
            var waiter = new Waiter<TResult>(msg.MessageId);
            _response[msg.MessageId] = waiter;
            await SendItem(query);
            await waiter.Task;
            return ((dynamic) waiter.Task).Result;
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

            public abstract void SetCancelled();
        }

        private class Waiter<T> : Waiter
        {
            private readonly TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();

            public Waiter(Guid id)
                : base(id)
            {
            }

            public override Task Task => _completionSource.Task;

            public override void Trigger(object result)
            {
                if (result is Exception exception)
                {
                    if (exception is AggregateException)
                        _completionSource.SetException(new ServerSideException("Server failed to execute.",
                            ((AggregateException) exception).InnerException));
                    else
                        _completionSource.SetException(exception);
                }

                else
                    _completionSource.SetResult((T) result);
            }

            public override void SetCancelled()
            {
                _completionSource.SetCanceled();
            }
        }

        /// <summary>
        ///     Start client (will auto connect if getting disconnected)
        /// </summary>
        /// <param name="address">The address for the CQS server.</param>
        /// <param name="port">The port that the CQS server is listening on.</param>
        /// <returns></returns>
        public async Task StartAsync(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
        }

        private int AuthenticateBytes(ITcpChannel channel, ISocketBuffer buffer)
        {
            var bytesProcessed = Authenticator.Process(channel, buffer, out var completed);
            if (completed)
                channel.BufferPreProcessor = null;
            return bytesProcessed;
        }

        private async Task EnsureConnected()
        {
            if (!_client.IsConnected)
            {
                if (_endPoint == null)
                    throw new InvalidOperationException("Call 'Start()' first.");

                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
            }
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
                    {
                        _response.TryRemove(waiter.Id, out removed);
                        waiter.SetCancelled();
                    }
                }
            }
            catch (Exception)
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
            if (message is AuthenticationRequiredException || _continueAuthenticate)
            {
                var authenticator = Authenticator;
                if (authenticator is { AuthenticationFailed: false })
                {
                    if (authenticator.RequiresRawData)
                        channel.BufferPreProcessor = AuthenticateBytes;
                    else
                    {
                        _continueAuthenticate = authenticator.Process(channel, message);
                        if (!_continueAuthenticate)
                        {
                            if (_lastSentItem != null)
                                channel.Send(_lastSentItem);
                        }
                    }
                }
            }

            if (_response.TryRemove(message., out var waiter))
            {
                //TODO: LOG
                return ClientFilterResult.Revoke;
            }
            waiter.Trigger(result.Body);

            //currently authentication etc are not wrapped, so we need to do it like this.
            if (message is not ClientResponse)
            {
                if (_response.Count != 1)
                    throw new InvalidOperationException(
                        "More than one pending message and we received an unwrapped object: " + message);

                var key = _response.Keys.First();
                if (!_response.TryRemove(key, out var waiter))
                {
                    //TODO: LOG
                    return ClientFilterResult.Revoke;
                }
                waiter.Trigger(message);
            }

            else
            {

            }


            return ClientFilterResult.Revoke;
        }

        private Task SendItem(object item)
        {
            _lastSentItem = item;
            return _client.SendAsync(item);
        }

        public async Task SendAsync(ClaimsPrincipal principal, object message)
        {
            var msg = new Message(message);
            msg.Properties["MessageType"] = "MESSAGE";
            await EnsureConnected();
            await SendItem(msg);
        }

        public async Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            await EnsureConnected();
            await SendItem(message);
        }

        public async Task SendAsync(Message message)
        {
            await EnsureConnected();
            await SendItem(message);
        }

        public async Task SendAsync(object message)
        {
            var msg = new Message(message);
            msg.Properties["MessageType"] = "MESSAGE";
            await SendAsync(msg);
        }
    }
}