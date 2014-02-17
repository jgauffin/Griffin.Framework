using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Client want to disconect.
    /// </summary>
    /// <remarks>
    /// Package is sent to make sure that all packages before this one have been processed.
    /// </remarks>
    public class DisconnectHandler : IFrameHandler
    {
        public IFrame Process(IStompClient client, IFrame request)
        {
            var id = request.Headers["receipt"];
            if (string.IsNullOrEmpty(id))
                throw new BadRequestException(request, "Missing the 'receipt' header in the frame. It's required so that we can notify you when the DISCONNECT frame has been received.");

            if (client.HasActiveTransactions)
                throw new BadRequestException(request,
                    "Got pending transactions. Just close the socket to abort them, or send proper commits/rollbacks before the DISCONNECT frame..");

            var response = new BasicFrame("RECEIPT");
            response.AddHeader("receipt-id", id);
            return response;
        }
    }
}
