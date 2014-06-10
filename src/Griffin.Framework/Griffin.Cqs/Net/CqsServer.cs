using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.MicroMsg.Serializers;

namespace Griffin.Cqs.Net
{
    /// <summary>
    ///     The server receives Command/query objects from the other side. It will execute them and return the response (if
    ///     any).
    /// </summary>
    public class CqsServer
    {
        private readonly ICommandBus _commandBus;
        private readonly MethodInfo _commandMethod;
        private readonly IEventBus _eventBus;
        private readonly IQueryBus _queryBus;
        private readonly MethodInfo _queryMethod;
        private readonly MethodInfo _requestMethod;
        private readonly IRequestReplyBus _requestReplyBus;
        private ChannelTcpListener _listener;
        private Func<IMessageSerializer> _serializerFactory = () => new DataContractMessageSerializer();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsServer" /> class.
        /// </summary>
        public CqsServer(ICommandBus commandBus, IQueryBus queryBus, IEventBus eventBus,
            IRequestReplyBus requestReplyBus)
        {
            _commandBus = commandBus;
            _queryBus = queryBus;
            _eventBus = eventBus;
            _requestReplyBus = requestReplyBus;
            var config = new ChannelTcpListenerConfiguration(CreateDecoder, CreateEncoder);
            _listener = new ChannelTcpListener(config);
            _listener.MessageReceived = OnClientMessage;
            _requestMethod = GetType().GetMethod("ExecuteRequest", BindingFlags.NonPublic | BindingFlags.Instance);
            _commandMethod = GetType().GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            _queryMethod = GetType().GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public CqsServer(IMessagingListener messagingListener)
        {
        }

        /// <summary>
        ///     Allows you to select how you want your objects to be serialized.
        /// </summary>
        public Func<IMessageSerializer> SerializerFactory
        {
            get { return _serializerFactory; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _serializerFactory = value;
            }
        }


        private Task ExecuteCommand<T>(T command) where T : Command
        {
            return _commandBus.ExecuteAsync(command);
        }

        private Task ExecuteQuery<T, TResult>(T query) where T : Query<TResult>
        {
            return _queryBus.QueryAsync(query);
        }

        private Task ExecuteRequest<T, TResult>(T query) where T : Request<TResult>
        {
            return _requestReplyBus.ExecuteAsync(query);
        }

        private void OnClientMessage(ITcpChannel channel, object message)
        {
            if (message is IQuery)
            {
                var dto = (IQuery) message;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _queryMethod.MakeGenericMethod(type, replyType);
                var task = (Task) method.Invoke(this, new object[] {message});
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        channel.Send(new ClientResponse(dto.QueryId, task.Exception));
                    else
                        channel.Send(new ClientResponse(dto.QueryId, ((dynamic) task).Result));
                });
            }
            else if (message is Command)
            {
                var dto = (Command) message;
                var type = message.GetType();
                var method = _commandMethod.MakeGenericMethod(type);
                var task = (Task) method.Invoke(this, new object[] {message});
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        channel.Send(new ClientResponse(dto.CommandId, task.Exception));
                    else
                        channel.Send(new ClientResponse(dto.CommandId, ((dynamic) task).Result));
                });
            }
            if (message is IRequest)
            {
                var dto = (IRequest) message;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _requestMethod.MakeGenericMethod(type, replyType);
                var task = (Task) method.Invoke(this, new object[] {message});
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        channel.Send(new ClientResponse(dto.RequestId, task.Exception));
                    else
                        channel.Send(new ClientResponse(dto.RequestId, ((dynamic) task).Result));
                });
            }
            if (message is ApplicationEvent)
            {
                var dto = (ApplicationEvent) message;
                var task = _eventBus.PublishAsync(dto);
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        channel.Send(new ClientResponse(dto.EventId, task.Exception));
                    else
                        channel.Send(new ClientResponse(dto.EventId, ((dynamic) task).Result));
                });
            }
        }

        private IMessageEncoder CreateEncoder()
        {
            return new MicroMessageEncoder(_serializerFactory());
        }

        private IMessageDecoder CreateDecoder()
        {
            return new MicroMessageDecoder(_serializerFactory());
        }
    }
}