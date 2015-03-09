using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Net;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs
{
   /// <summary>
    /// Used to execute CQS messages server side.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the different bus's in the <see cref="Griffin.Cqs.Simple"/> namespace unless you configure the properties
    /// in this class.
    /// </para>
    /// </remarks>
    public class CqsMessageProcessor
    {
        private readonly MethodInfo _commandMethod;
        private readonly MethodInfo _queryMethod;
        private readonly MethodInfo _requestMethod;
        private ICommandBus _commandBus;
        private IEventBus _eventBus;
        private IQueryBus _queryBus;
        private IRequestReplyBus _requestReplyBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="CqsMessageProcessor"/> class.
        /// </summary>
        public CqsMessageProcessor()
        {
            CommandBus = new SimpleCommandBus();
            QueryBus = new SimpleQueryBus();
            EventBus = new SimpleEventBus();
            RequestReplyBus = new SimpleRequestReplyBus();
            _requestMethod = GetType().GetMethod("ExecuteRequest", BindingFlags.NonPublic | BindingFlags.Instance);
            _commandMethod = GetType().GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            _queryMethod = GetType().GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Command bus implementation to use for inbound commands.
        /// </summary>
        public ICommandBus CommandBus
        {
            get { return _commandBus; }
            set { _commandBus = value; }
        }


        /// <summary>
        /// Query bus implementation to use for inbound queries.
        /// </summary>
        public IQueryBus QueryBus
        {
            get { return _queryBus; }
            set { _queryBus = value; }
        }


        /// <summary>
        /// Event bus implementation to use for inbound application events.
        /// </summary>
        public IEventBus EventBus
        {
            get { return _eventBus; }
            set { _eventBus = value; }
        }


        /// <summary>
        /// Request/reply bus implementation to use for inbound requests.
        /// </summary>
        public IRequestReplyBus RequestReplyBus
        {
            get { return _requestReplyBus; }
            set { _requestReplyBus = value; }
        }

      
        /// <summary>
        ///     ProcessAsync message
        /// </summary>
        /// <param name="message">CQS message to process</param>
        /// <returns>If message processing can continue</returns>
        public async Task<ClientResponse> ProcessAsync(object message)
        {
            if (message is IQuery)
            {
                var dto = (IQuery)message;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _queryMethod.MakeGenericMethod(type, replyType);
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    return new ClientResponse(dto.QueryId, ((dynamic)task).Result);
                }
                catch (TargetInvocationException exception)
                {
                    return new ClientResponse(dto.QueryId, exception.InnerException);
                }
            }

            if (message is Command)
            {
                var dto = (Command)message;
                var type = message.GetType();
                var method = _commandMethod.MakeGenericMethod(type);
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    return new ClientResponse(dto.CommandId, null);
                }
                catch (TargetInvocationException exception)
                {
                    return new ClientResponse(dto.CommandId, exception.InnerException);
                }

            }

            if (message is IRequest)
            {
                var dto = (IRequest)message;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _requestMethod.MakeGenericMethod(type, replyType);
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    return new ClientResponse(dto.RequestId, ((dynamic)task).Result);
                }
                catch (TargetInvocationException exception)
                {
                    return new ClientResponse(dto.RequestId, exception.InnerException);
                }

            }

            if (message is ApplicationEvent)
            {
                var dto = (ApplicationEvent)message;
                try
                {
                    var task = _eventBus.PublishAsync(dto);
                    await task;
                    return new ClientResponse(dto.EventId, null);
                }
                catch (TargetInvocationException exception)
                {
                    return new ClientResponse(dto.EventId, exception.InnerException);
                }
            }

            throw new InvalidOperationException("Unknown type: " + message.GetType().FullName);
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
    }
}
