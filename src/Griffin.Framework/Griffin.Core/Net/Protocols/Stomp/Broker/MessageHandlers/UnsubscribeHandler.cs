using System;
using Griffin.Net.Protocols.Stomp.Broker.Services;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    public class UnsubscribeHandler : IFrameHandler
    {
        private IQueueRepository _queueRepository;

        public UnsubscribeHandler(IQueueRepository queueRepository)
        {
            if (queueRepository == null) throw new ArgumentNullException("queueRepository");
            _queueRepository = queueRepository;
        }

        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["id"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the ID header in the frame.");

            var subscription = client.RemoveSubscription(id);
            if (subscription == null)
                throw new BadRequestException(request, string.Format("Failed to find an subscription with id '{0}'.", id));

            var queue = _queueRepository.Get(subscription.QueueName);
            if (queue == null)
            {
                //TODO: Log that the queue do not exist (even though our subscription existed).
                return null;
            }

            queue.Unbsubscribe(subscription);
            return null;
        }
    }
}
