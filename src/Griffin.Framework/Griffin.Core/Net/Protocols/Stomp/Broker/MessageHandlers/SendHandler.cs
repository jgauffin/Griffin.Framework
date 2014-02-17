using System;
using System.IO;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Takes care of messages that a client want to send to a queue.
    /// </summary>
    public class SendHandler : IFrameHandler
    {
        private readonly IQueueRepository _queueRepository;

        public SendHandler(IQueueRepository queueRepository)
        {
            if (queueRepository == null) throw new ArgumentNullException("queueRepository");

            _queueRepository = queueRepository;
        }

        public IFrame Process(IStompClient client, IFrame request)
        {
            ValidateContentHeaders(request);

            var queue = GetQueue(request);
            var message = CreateOutboundMessage(client, request);

            var transactionId = request.Headers["transaction"];
            if (!string.IsNullOrEmpty(transactionId))
            {
                client.EnqueueInTransaction(transactionId, () => queue.Enqueue(message), () => { });
                return null;
            }

            return message;
        }

        private IStompQueue GetQueue(IFrame request)
        {
            var destinationName = request.Headers["destination"];
            if (string.IsNullOrEmpty(destinationName))
                throw new BadRequestException(request, "'destination' header is empty.");
            return _queueRepository.Get(destinationName);
        }

        private static void ValidateContentHeaders(IFrame request)
        {
            var contentType = request.Headers["content-type"];
            if (string.IsNullOrEmpty(contentType) && request.Body != null)
                throw new BadRequestException(request, "SEND frames with a body MUST have a 'content-type' header.");

            var contentLength = request.Headers["content-length"];
            if (contentLength == "0")
                contentLength = "";
            if (string.IsNullOrEmpty(contentLength) && request.Body != null)
                throw new BadRequestException(request, "SEND frames with a body MUST have a 'content-length' header.");
        }

        private static BasicFrame CreateOutboundMessage(IStompClient client, IFrame request)
        {
            var message = new BasicFrame("MESSAGE");
            foreach (var header in request.Headers)
            {
                message.AddHeader(header.Key, header.Value);
            }
            message.AddHeader("Originator-Session", client.SessionKey);
            message.AddHeader("Originator-Address", client.RemoteEndpoint.ToString());

            if (request.Body != null)
            {
                message.Body = new MemoryStream();
                request.Body.CopyTo(message.Body);
                message.Body.Position = 0;
            }
            return message;
        }
    }
}
