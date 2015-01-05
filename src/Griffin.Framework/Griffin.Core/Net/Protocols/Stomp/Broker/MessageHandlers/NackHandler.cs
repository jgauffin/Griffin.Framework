using Griffin.Net.Protocols.Stomp.Broker.Services;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Processes the NACK frame
    /// </summary>
    public class NackHandler : IFrameHandler
    {
       private readonly IQueueRepository _queueRepository;

       /// <summary>
       /// Initializes a new instance of the <see cref="NackHandler"/> class.
       /// </summary>
       /// <param name="queueRepository">The queue repository.</param>
        public NackHandler(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Removes the frame from the client pending list and adds it back to the queue
        /// </summary>
        /// <param name="client">Connection that received the frame</param>
        /// <param name="request">Inbound frame to process</param>
        /// <returns>
        /// Frame to send back
        /// </returns>
        /// <exception cref="BadRequestException">
        /// Missing the 'id' header in the frame. Required so that we know which message that the NACK is for.
        /// or
        /// </exception>
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["id"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'id' header in the frame. Required so that we know which message that the NACK is for.");

            if (!client.IsFramePending(id))
                throw new BadRequestException(request, string.Format("Unknown message with id '{0}'. can therefore not NACK it.", id));

            var subscription = client.GetSubscription(id);

            var transactionId = request.Headers["transaction"];
            if (!string.IsNullOrEmpty(transactionId))
            {
                client.EnqueueInTransaction(transactionId, () => NackMessages(subscription, id), () => {});
                return null;
            }

            NackMessages(subscription, id);
            return null;
        }

        private void NackMessages(Subscription subscription, string id)
        {
            var nackedFrames = subscription.Nack(id);
            var queue = _queueRepository.Get(subscription.QueueName);
            foreach (var frame in nackedFrames)
            {
                //TODO: Should messages be put in front of the queue?
                queue.Enqueue(frame);
            }
        }
    }
}
