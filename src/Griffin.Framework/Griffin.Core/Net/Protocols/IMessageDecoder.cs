using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    ///     Decodes incoming bytes into something more useful.
    /// </summary>
    public interface IMessageDecoder
    {
        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="channel">Used to receive bytes when required</param>
        /// <param name="receiveBuffer">Buffer to used when receiving data. Might already contain data, check Offset and Count.</param>
        /// <remarks>
        ///     <para>
        ///         The receiveBuffer are shared between everything that uses a channel. The offset indicates the amount of
        ///         consumed bytes, while Count tells how many bytes where originally read from the socket. Increase the offset
        ///         for every byte that you read. You can use <see cref="SegmentExtensions.BytesLeft"/> to get the amount of available bytes.
        ///     </para>
        /// </remarks>
        Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment receiveBuffer);
    }
}