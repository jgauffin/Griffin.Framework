namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Commit transaction
    /// </summary>
    public class CommitHandler : IFrameHandler
    {
        /// <summary>
        /// Process an inbound frame.
        /// </summary>
        /// <param name="client">Connection that received the frame</param>
        /// <param name="request">Inbound frame to process</param>
        /// <returns>
        /// Frame to send back; <c>null</c> if no message should be returned;
        /// </returns>
        /// <exception cref="BadRequestException">Missing the 'transaction' header in the frame.</exception>
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["transaction"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'transaction' header in the frame.");


            client.CommitTransaction(id);
            return null;
        }
    }
}