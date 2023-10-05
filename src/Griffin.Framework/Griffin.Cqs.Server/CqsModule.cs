using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Net;
using Griffin.Cqs.Simple;
using Griffin.Net.LiteServer;
using Griffin.Net.LiteServer.Modules;

namespace Griffin.Cqs.Server
{
    /// <summary>
    /// Used to execute CQS messages in a <see cref="LiteServer"/>.
    /// </summary>
    public class CqsModule : IServerModule
    {
        private readonly MethodInfo _messageMethod;
        private readonly MethodInfo _queryMethod;

        public CqsModule()
        {
            MessageBus = new SimpleMessageBus();
            QueryBus = new SimpleQueryBus();
            _messageMethod = GetType().GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            _queryMethod = GetType().GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public IMessageBus MessageBus { get; set; }

        public IQueryBus QueryBus { get; set; }

        /// <summary>
        ///     Begin request is always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
#pragma warning disable 1998
        public async Task BeginRequestAsync(IClientContext context)
#pragma warning restore 1998
        {
        }

        /// <summary>
        ///    The client expects a ClientResponse to be able to map responses to requests.
        /// </summary>
        /// <param name="context">Context information</param>
#pragma warning disable 1998
        public async Task EndRequest(IClientContext context)
#pragma warning restore 1998
        {
            if (context.ResponseMessage.Body is ClientResponse)
                return;

            if (context.ResponseMessage == null && context.Error != null)
                context.ResponseMessage = context.Error;

            var id = context.RequestData["Id"];
            if (id == null)
            {
                id = CqsExtensions.GetId(context.RequestMessage);
            }

            context.ResponseMessage = new ClientResponse((Guid)id, context.ResponseMessage.Body);
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
            var messageType = message.Properties["Message-Type"];
            if (messageType ==  "QUERY")
            {
                var dto = message.Body;
                var type = message.GetType();
                var replyType = type.BaseType.GetGenericArguments()[0];
                var method = _queryMethod.MakeGenericMethod(type, replyType);
                context.RequestData["Id"] = message.MessageId;
                try
                {
                    var task = (Task)method.Invoke(this, new[] { message });
                    await task;
                    context.ResponseMessage = new ClientResponse(message.MessageId, ((dynamic)task).Result);
                }
                catch (TargetInvocationException exception)
                {
                    context.ResponseMessage = new ClientResponse(message.MessageId, exception.InnerException);
                }

                return ModuleResult.SendResponse;
            }

            if (messageType == "MESSAGE")
            {
                var dto = message;
                var type = message.GetType();
                var method = _messageMethod.MakeGenericMethod(type);
                context.RequestData["Id"] = message.MessageId;
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

            if (messageType == "REPLY")
            {
                //TODO
                return ModuleResult.SendResponse;
            }

            return ModuleResult.Continue;
        }


        private Task ExecuteQuery<T, TResult>(T query) where T : Query<TResult>
        {
            return QueryBus.QueryAsync(query);
        }

    }
}