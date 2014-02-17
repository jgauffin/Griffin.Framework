namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Commit transaction
    /// </summary>
    public class CommitHandler : IFrameHandler
    {
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