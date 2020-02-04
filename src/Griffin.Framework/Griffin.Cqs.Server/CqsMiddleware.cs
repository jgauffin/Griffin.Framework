using System;
using System.Threading.Tasks;
using Griffin.Net.LiteServer.Modules.Authentication;

namespace Griffin.Cqs.Server
{
    /// <summary>
    ///     Used to execute CQS messages in a <see cref="LiteServer" />.
    /// </summary>
    public class CqsMiddleware : MicroMessageMiddleware
    {
        private readonly CqsMessageProcessor _messageProcessor;

        public CqsMiddleware(CqsMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public override async Task Process(MicroMessageContext context, Func<Task> next)
        {
            if (!CqsObjectMapper.IsCqsType(context.RequestMessage.GetType()))
            {
                await next();
                return;
            }

            context.RequestMessage = await _messageProcessor.ProcessAsync(context.RequestMessage);
        }
    }
}