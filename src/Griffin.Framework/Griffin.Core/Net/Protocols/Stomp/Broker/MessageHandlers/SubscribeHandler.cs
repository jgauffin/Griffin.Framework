using Griffin.Net.Protocols.Stomp.Broker.Services;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Takes care of the SUBSCRIBE frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adds the current connection to the queue subscribers.
    /// </para>
    /// </remarks>
    public class SubscribeHandler : IFrameHandler
    {
        private readonly IQueueRepository _queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeHandler"/> class.
        /// </summary>
        /// <param name="queueRepository">The queue repository.</param>
        public SubscribeHandler(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Process an inbound frame.
        /// </summary>
        /// <param name="client">Client that sent the frame</param>
        /// <param name="request">Frame to process</param>
        /// <returns>Frame to send back</returns>
        /// <exception cref="BadRequestException"></exception>
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["id"];
            if (id == null)
                throw new BadRequestException(request, "You must include the 'id' header in the SUBSCRIBE frame.");

            if (client.SubscriptionExists(id))
                throw new BadRequestException(request, string.Format("There is already a subscription with id '{0}'.", id));

            var ackType = GetAckType(request);
            var queue= GetQueue(request);

            var subscription = new Subscription(client, id)
            {
                AckType = ackType,
                QueueName = queue.Name
            };

            queue.AddSubscription(subscription);
            client.AddSubscription(subscription);

            return request.CreateReceiptIfRequired();
        }

        private IStompQueue GetQueue(IFrame request)
        {
            var queueName = request.Headers["destination"];
            if (queueName == null)
                throw new BadRequestException(request, "You must specify the 'destination' header in the SUBSCRIBE frame.");

            return _queueRepository.Get(queueName);
        }

        private static string GetAckType(IFrame request)
        {
            var ackType = request.Headers["ack"];
            if (ackType == null)
                ackType = "auto";

            switch (ackType)
            {
                case "auto":
                case "client":
                case "client-individual":
                    break;

                default:
                    throw new BadRequestException(request,
                        string.Format(
                            "Unknown ack type '{0}', must be one of: auto, client, or client-individual.", ackType));
            }
            return ackType;
        }
    }
}
