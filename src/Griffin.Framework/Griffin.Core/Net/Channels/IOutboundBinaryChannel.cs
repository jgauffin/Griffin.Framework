using System.Collections.Generic;
using System.Threading.Tasks;
using Griffin.Net.Buffers;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Channel which can send data.
    /// </summary>
    public interface IOutboundBinaryChannel
    {
        /// <summary>
        ///     Send something to the remote end point.
        /// </summary>
        /// <param name="buffer">Buffer to send.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="count">Amount fo bytes to send, starting at offset.</param>
        /// <remarks>
        ///     <para>
        ///         All pending buffers from <c>SendMore()</c> will be included in this write operation.
        ///     </para>
        ///     <para>
        ///         Buffers are guaranteed to be completely written to the socket when this method returns.
        ///     </para>
        /// </remarks>
        Task SendAsync(byte[] buffer, int offset, int count);

        /// <summary>
        ///     Send a collection of buffers.
        /// </summary>
        /// <param name="buffers">buffers.</param>
        /// <remarks>
        ///     <para>
        ///         The offset/count should point at the section in the slice that should be sent. Typically the <c>Offset</c> is
        ///         the same as <c>BaseOffset</c>.
        ///     </para>
        /// </remarks>
        Task SendAsync(IEnumerable<IBufferSegment> buffers);

        /// <summary>
        ///     Send a collection of buffers.
        /// </summary>
        /// <param name="buffer">Buffer to send</param>
        /// <remarks>
        ///     <para>
        ///         The offset/count should point at the section in the slice that should be sent. Typically the <c>Offset</c> is
        ///         the same as <c>BaseOffset</c>.
        ///     </para>
        /// </remarks>
        Task SendAsync(IBufferSegment buffer);

        /// <summary>
        ///     Enqueue something for delivery (will not do an actual send operation but enqueue data for delivery).
        /// </summary>
        /// <param name="buffer">Buffer to send.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="count">Amount fo bytes to send, starting at offset.</param>
        /// <remarks>
        ///     <para>
        ///         Nothing will be send until the <c>Send()</c> method have been invoked.
        ///     </para>
        /// </remarks>
        Task SendMoreAsync(byte[] buffer, int offset, int count);

        /// <summary>
        ///     Enqueue something for delivery (will not do an actual send operation but enqueue data for delivery).
        /// </summary>
        /// <param name="segment">Segment to send.</param>
        /// <remarks>
        ///     <para>
        ///         The offset/count should point at the section in the slice that should be sent. Typically the <c>Offset</c> is
        ///         the same as <c>BaseOffset</c>.
        ///     </para>
        ///     <para>
        ///         Nothing will be send until the <c>Send()</c> method have been invoked.
        ///     </para>
        ///     <para>
        ///         The buffer will be returned to the pool once the Send operation completes.
        ///     </para>
        /// </remarks>
        Task SendMoreAsync(IBufferSegment segment);
    }
}