using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Authentication;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

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
    public class CqsClient : ICommandBus, IEventBus, IQueryBus, IRequestReplyBus, IDisposable
    {
        private readonly Timer _cleanuptimer;
        private MicroMessageClient _microMessageClient;
        private readonly ConcurrentDictionary<Guid, Waiter> _response = new ConcurrentDictionary<Guid, Waiter>();
        private bool _continueAuthenticate;
        private IPEndPoint _endPoint;
        private object _lastSentItem;
        BufferManager _bufferManager = new BufferManager(10);

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
        /// </summary>
        public CqsClient()
        {
            _microMessageClient = new MicroMessageClient(new DataContractMessageSerializer(), _bufferManager);
            _cleanuptimer = new Timer(OnCleanup, 0, 10000, 10000);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
        /// </summary>
        /// <param name="serializer">Serializer to be used for the transported messages.</param>
        public CqsClient(Func<IMessageSerializer> serializer)
        {
            _microMessageClient = new MicroMessageClient(serializer(), _bufferManager);
            _cleanuptimer = new Timer(OnCleanup);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
        /// </summary>
        public CqsClient(ClientSideSslStreamBuilder sslStreamBuilder)
        {
            _microMessageClient = new MicroMessageClient(new DataContractMessageSerializer(), sslStreamBuilder);
            _cleanuptimer = new Timer(OnCleanup, 0, 10000, 10000);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsClient" /> class.
        /// </summary>
        /// <param name="serializer">Serializer to be used for the transported messages.</param>
        public CqsClient(Func<IMessageSerializer> serializer, ClientSideSslStreamBuilder streamBuilder)
        {
            _microMessageClient = new MicroMessageClient(serializer(), streamBuilder);
            _cleanuptimer = new Timer(OnCleanup);
        }

        /// <summary>
        ///     Set if you want to authenticate against a server.
        /// </summary>
        public IClientAuthenticator Authenticator { get; set; }


        /// <summary>
        ///     Execute a command and wait for result (i.e. exception for failure or just return for success)
        /// </summary>
        /// <typeparam name="T">Type of command</typeparam>
        /// <param name="command">command object</param>
        /// <returns>completion task</returns>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            await EnsureConnected();
            var waiter = new Waiter<T>(command.CommandId);
            _response[command.CommandId] = waiter;
            await SendItem(command);
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
        ///     Publishes an application event (at server side)
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
            await SendItem(e);
            await waiter.Task;
        }

        /// <summary>
        ///     Queries the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">query to get a response for.</param>
        /// <returns>completion task</returns>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            await EnsureConnected();
            var waiter = new Waiter<TResult>(query.QueryId);
            _response[query.QueryId] = waiter;
            await SendItem(query);
            await waiter.Task;
            return ((dynamic)waiter.Task).Result;
        }

        /// <summary>
        ///     Execute a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">The type of the reply.</typeparam>
        /// <param name="request">Request to get a reply for.</param>
        /// <returns>completion task</returns>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            await EnsureConnected();
            var waiter = new Waiter<TReply>(request.RequestId);
            _response[request.RequestId] = waiter;
            await SendItem(request);
            await waiter.Task;
            return ((dynamic)waiter.Task).Result;
        }

        private abstract class Waiter
        {
            protected Waiter(Guid id)
            {
                Id = id;
                WaitedSince = DateTime.UtcNow;
            }

            public Guid Id { get; private set; }

            private DateTime WaitedSince { get; }
            public abstract Task Task { get; }

            public bool Expired => DateTime.UtcNow.Subtract(WaitedSince).TotalSeconds > 10;

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
                if (result is Exception)
                {
                    if (result is AggregateException ae)
                        _completionSource.SetException(new ServerSideException("Server failed to execute.",
                            ae.InnerException));
                    else
                        _completionSource.SetException((Exception)result);
                }

                else
                    _completionSource.SetResult((T)result);
            }

            public override void SetCancelled()
            {
                _completionSource.SetCanceled();
            }
        }

        /// <summary>
        ///     Start client (will reconnect if getting disconnected)
        /// </summary>
        /// <param name="address">The address for the CQS server.</param>
        /// <param name="port">The port that the CQS server is listening on.</param>
        /// <returns></returns>
        public async Task StartAsync(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            await EnsureConnected();
        }

        private async Task EnsureConnected()
        {
            if (_microMessageClient.IsConnected)
            {
                return;
            }

            if (_endPoint == null)
                throw new InvalidOperationException("Call 'Start()' first.");

            await _microMessageClient.ConnectAsync(_endPoint.Address, _endPoint.Port);

#pragma warning disable 4014
            _microMessageClient.ReceiveAsync().ContinueWith(OnResponse);
#pragma warning restore 4014
        }

        private void OnResponse(Task<object> obj)
        {
            var msg = obj.Result;
            try
            {
                ProcessInboundMessage(obj, msg);
            }
            catch (Exception ex)
            {

            }

            try
            {
                _microMessageClient.ReceiveAsync().ContinueWith(OnResponse);
            }
            catch (Exception ex)
            {

            }
        }

        private void ProcessInboundMessage(Task<object> obj, object msg)
        {
            var result = msg as ClientResponse;
            if (result == null)
            {
                if (_response.Count != 1)
                    throw new InvalidOperationException(
                        "More than one pending message and we received an unwrapped object: " + obj.Result);

                var key = _response.Keys.First();
                if (!_response.TryRemove(key, out var waiter))
                {
                    //TODO: LOG
                    return;
                }

                waiter.Trigger(obj.Result);
            }

            else
            {
                if (!_response.TryRemove(result.Identifier, out var waiter))
                {
                    //TODO: LOG
                    return;
                }

                waiter.Trigger(result.Body);
            }
        }

        private void OnCleanup(object state)
        {
            try
            {
                var values = _response.Values;
                foreach (var waiter in values)
                {
                    if (!waiter.Expired)
                        continue;

                    _response.TryRemove(waiter.Id, out _);
                    waiter.SetCancelled();
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
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<bool> ReadAuthenticationMessages(object message)
        {
            if (!(message is AuthenticationRequiredException) && !_continueAuthenticate)
            {
                return false;
            }

            await Authenticator.ProcessAsync(_microMessageClient, message);
            _continueAuthenticate = await Authenticator.ProcessAsync(_microMessageClient, message);
            if (_continueAuthenticate) 
                return _continueAuthenticate;

            if (_lastSentItem != null)
                await _microMessageClient.SendAsync(_lastSentItem);

            return false;
        }

        private Task SendItem(object item)
        {
            _lastSentItem = item;
            return _microMessageClient.SendAsync(item);
        }
    }
}