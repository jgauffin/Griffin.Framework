using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public static class FrameExtensions
    {
        public static IFrame CreateReceiptIfRequired(this IFrame request)
        {
            var receipt = request.Headers["receipt"];
            if (receipt != null)
            {
                var response = new BasicFrame("RECEIPT");
                response.Headers["receipt-id"] = receipt;
                return response;
            }

            return null;
        }
    }
}
