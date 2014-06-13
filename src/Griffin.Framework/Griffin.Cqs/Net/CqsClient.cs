using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Cqs.Net
{
    public class CqsClient : ICommandBus, IEventBus, IQueryBus, IRequestReplyBus, IDisposable
    {
        private readonly Timer _cleanuptimer;
        private readonly ChannelTcpClient<object> _client;
        private readonly ConcurrentDictionary<Guid, Waiter> _response = new ConcurrentDictionary<Guid, Waiter>();
        private IPEndPoint _endPoint;

        public CqsClient()
        {
            _client = new ChannelTcpClient<object>(
                new MicroMessageEncoder(new DataContractMessageSerializer()),
                new MicroMessageDecoder(new DataContractMessageSerializer()));
            _client.Filter = OnMessageReceived;
            _cleanuptimer = new Timer(OnCleanup, 0, 10000, 10000);
        }

        public CqsClient(Func<IMessageSerializer> serializer)
        {
            _client = new ChannelTcpClient<object>(new MicroMessageEncoder(serializer()),
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

        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e)
            where TApplicationEvent : ApplicationEvent
        {
            await EnsureConnected();
            var waiter = new Waiter<TApplicationEvent>(e.EventId);
            _response[e.EventId] = waiter;
            await _client.SendAsync(e);
            await waiter.Task;
        }

        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            await EnsureConnected();
            var waiter = new Waiter<TResult>(query.QueryId);
            _response[query.QueryId] = waiter;
            await _client.SendAsync(query);
            await waiter.Task;
            return ((dynamic) waiter.Task).Result;
        }

        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            await EnsureConnected();
            var waiter = new Waiter<TReply>(request.RequestId);
            _response[request.RequestId] = waiter;
            await _client.SendAsync(request);
            await waiter.Task;
            return ((dynamic) waiter.Task).Result;
        }

        public async Task StartAsync(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
        }

        private async Task EnsureConnected()
        {
            if (!_client.IsConnected)
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
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
            var result = (ClientResponse) message;
            Waiter waiter;
            if (!_response.TryRemove(result.Identifier, out waiter))
            {
                //TODO: LOG
                return ClientFilterResult.Revoke;
            }

            waiter.Trigger(result.Body);

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

            public Waiter(Guid id) : base(id)
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
                            ((AggregateException) result).InnerException));
                    else
                        _completionSource.SetException((Exception) result);
                }

                else
                    _completionSource.SetResult((T) result);
            }
        }
    }
}