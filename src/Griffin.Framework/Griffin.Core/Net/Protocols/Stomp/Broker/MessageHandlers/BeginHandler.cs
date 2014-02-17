namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Begin a new transaction
    /// </summary>
    public class BeginHandler : IFrameHandler
    {
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["transaction"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'transaction' header in the frame.");

            client.BeginTransaction(id);
            return null;
        }
    }
}
