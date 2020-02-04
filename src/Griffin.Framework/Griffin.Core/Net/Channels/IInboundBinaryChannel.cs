using System.Threading.Tasks;
using Griffin.Net.Buffers;

namespace Griffin.Net.Channels
{
    public interface IInboundBinaryChannel
    {
        /// <summary>
        ///     Receive data from the socket.
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         Will receive from the <see cref="IBufferSegment.Offset" /> and the remaining capacity. Count will be increased
        ///         with the amount of read bytes.
        ///     </para>
        /// </remarks>
        Task<int> ReceiveAsync(IBufferSegment readBuffer);
    }
}