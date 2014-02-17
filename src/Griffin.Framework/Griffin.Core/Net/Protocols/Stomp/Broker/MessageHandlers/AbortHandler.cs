namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Rollback transaction
    /// </summary>
    public class AbortHandler : IFrameHandler
    {
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["transaction"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'transaction' header in the frame.");

            client.RollbackTransaction(id);
            return null;
        }
    }
}