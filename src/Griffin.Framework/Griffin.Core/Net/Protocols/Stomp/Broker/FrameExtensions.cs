using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// Extension methods for frames
    /// </summary>
    public static class FrameExtensions
    {
        /// <summary>
        /// Check if the 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
