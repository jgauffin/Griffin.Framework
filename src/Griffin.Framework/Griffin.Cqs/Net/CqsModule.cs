using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Net.Modules;
using Griffin.Cqs.Simple;
using Griffin.Net.Server;
using Griffin.Net.Server.Modules;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Used to execute CQS messages in a <see cref="LiteServer"/>.
    /// </summary>
    public class CqsModule : IServerModule
    {
        private readonly MethodInfo _commandMethod;
        private readonly MethodInfo _queryMethod;
        private readonly MethodInfo _requestMethod;
        private ICommandBus _commandBus;
        private IEventBus _eventBus;
        private IQueryBus _queryBus;
        private IRequestReplyBus _requestReplyBus;

        public CqsModule()
        {
            CommandBus = new SimpleCommandBus();
            QueryBus = new SimpleQueryBus();
            EventBus = new SimpleEventBus();
            RequestReplyBus = new SimpleRequestReplyBus();
            _requestMethod = GetType().GetMethod("ExecuteRequest", BindingFlags.NonPublic | BindingFlags.Instance);
            _commandMethod = GetType().GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            _queryMethod = GetType().GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public ICommandBus CommandBus
        {
            get { return _commandBus; }
            set { _commandBus = value; }
        }

        public IQueryBus QueryBus
        {
            get { return _queryBus; }
            set { _queryBus = value; }
        }

        public IEventBus EventBus
        {
            get { return _eventBus; }
            set { _eventBus = value; }
        }

        public IRequestReplyBus RequestReplyBus
        {
            get { return _requestReplyBus; }
            set { _requestReplyBus = value; }
        }

        /// <summary>
        ///     Begin request is always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        public async Task BeginRequestAsync(IClientContext context)
        {
        }

        /// <summary>
        ///    The client expects a ClientResposne to be able to map responses to requests.
        /// </summary>
        /// <param name="context">Context information</param>
        public async Task EndRequest(IClientContext context)
        {
            if (context.ResponseMessage is ClientResponse)
                return;

            if (context.ResponseMessage == null && context.Error != null)
                context.ResponseMessage = context.Error;

            var id = context.RequestData["Id"];
            if (id == null)
            {
                id = CqsExtensions.GetId(context.RequestMessage);
            }

            context.ResponseMessage = new ClientResponse((Guid)id, context.ResponseMessage);
        }

        /// <summary>
        ///     ProcessAsync message
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        /// <remarks>
        ///     <para>
        ///         Check the <see cref="ModuleResult" /> property to see how the message processing have gone so
        ///         far.
        ///     </para>
        /// </remarks>
        public async Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            var message = context.RequestMessage;

            if (message is IQuery)
            {
                var dto = (IQuery)message;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _queryMethod.MakeGenericMethod(type, replyType);
                context.RequestData["Id"] = dto.QueryId;
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    context.ResponseMessage = new ClientResponse(dto.QueryId, ((dynamic)task).Result);
                }
                catch (TargetInvocationException exception)
                {
                    context.ResponseMessage = new ClientResponse(dto.QueryId, exception.InnerException);
                }

                return ModuleResult.SendResponse;
            }

            if (message is Command)
            {
                var dto = (Command)message;
                var type = message.GetType();
                var method = _commandMethod.MakeGenericMethod(type);
                context.RequestData["Id"] = dto.CommandId;
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    context.ResponseMessage = new ClientResponse(dto.CommandId, null);
                }
                catch (TargetInvocationException exception)
                {
                    context.ResponseMessage = new ClientResponse(dto.CommandId, exception.InnerException);
                }

                return ModuleResult.SendResponse;
            }

            if (message is IRequest)
            {
                var dto = (IRequest)message;
                context.RequestData["Id"] = dto.RequestId;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _requestMethod.MakeGenericMethod(type, replyType);
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    context.ResponseMessage = new ClientResponse(dto.RequestId, ((dynamic)task).Result);
                }
                catch (TargetInvocationException exception)
                {
                    context.ResponseMessage = new ClientResponse(dto.RequestId, exception.InnerException);
                }

                return ModuleResult.SendResponse;
            }

            if (message is ApplicationEvent)
            {
                var dto = (ApplicationEvent)message;
                context.RequestData["Id"] = dto.EventId;
                try
                {
                    var task = _eventBus.PublishAsync(dto);
                    await task;
                    context.ResponseMessage = new ClientResponse(dto.EventId, null);
                }
                catch (TargetInvocationException exception)
                {
                    context.ResponseMessage = new ClientResponse(dto.EventId, exception.InnerException);
                }

                return ModuleResult.SendResponse;
            }

            return ModuleResult.Continue;
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