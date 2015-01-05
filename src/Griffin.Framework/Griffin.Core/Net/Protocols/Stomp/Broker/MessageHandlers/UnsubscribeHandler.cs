using System;
using Griffin.Net.Protocols.Stomp.Broker.Services;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Stop receiving messages from a queue
    /// </summary>
    public class UnsubscribeHandler : IFrameHandler
    {
        private IQueueRepository _queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsubscribeHandler"/> class.
        /// </summary>
        /// <param name="queueRepository">The queue repository.</param>
        /// <exception cref="System.ArgumentNullException">queueRepository</exception>
        public UnsubscribeHandler(IQueueRepository queueRepository)
        {
            if (queueRepository == null) throw new ArgumentNullException("queueRepository");
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Process an inbound frame.
        /// </summary>
        /// <param name="client">Connection that received the frame</param>
        /// <param name="request">Inbound frame to process</param>
        /// <returns>
        /// Frame to send back
        /// </returns>
        /// <exception cref="BadRequestException">
        /// Missing the ID header in the frame.
        /// or
        /// </exception>
        public IFrame Process(IStompClient client, IFrame request)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (request == null) throw new ArgumentNullException("request");
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

            queue.Unsubscribe(subscription);
            return null;
        }
    }
}
