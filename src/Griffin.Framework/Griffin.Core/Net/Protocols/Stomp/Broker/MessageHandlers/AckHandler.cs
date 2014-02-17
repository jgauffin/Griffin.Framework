namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    public class AckHandler : IFrameHandler
    {
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["id"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'id' header in the frame. Required so that we know which message that the ACK is for.");

            if (!client.IsFramePending(id))
                throw new BadRequestException(request, string.Format("Unknown message with id '{0}'. can therefore not ACK it.", id));

            var subscription = client.GetSubscription(id);
            var transactionId = request.Headers["transaction"];
            if (!string.IsNullOrEmpty(transactionId))
            {
                client.EnqueueInTransaction(transactionId, () => subscription.Ack(id), () => { });
                return null;
            }

            subscription.Ack(id);
            return null;
        }

    }
}
