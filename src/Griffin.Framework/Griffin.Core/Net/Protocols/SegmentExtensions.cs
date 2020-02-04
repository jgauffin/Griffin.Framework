using System;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Microsoft.Win32.SafeHandles;

namespace Griffin.Net.Protocols
{
    public static class SegmentExtensions
    {
        public static void WriteString(this IBufferSegment segment, string value, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var len = encoding.GetByteCount(value);
            if (segment.UnallocatedBytes() < len)
                throw new CodecException("Internal buffer is full.");

            encoding.GetBytes(value, 0, value.Length, segment.Buffer, segment.Offset);
            segment.Offset += len;
        }

        public static int BytesLeft(this IBufferSegment segment)
        {
            var read = segment.Offset - segment.StartOffset;
            return segment.Count - read;
        }

        public static int BytesProcessed(this IBufferSegment segment)
        {
            return segment.Offset - segment.StartOffset;
        }

        public static int UnallocatedBytes(this IBufferSegment segment)
        {
            var used = segment.Offset - segment.StartOffset;
            return segment.Capacity - used;
        }

        /// <summary>
        /// Fills the buffer with more information at the tail of existing data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        /// <remarks>
        ///<para>
        ///Will put the offset at the beginning of the data once the read completes.
        /// </para>
        /// </remarks>
        public static async Task ReceiveMore(this IBufferSegment buffer, IInboundBinaryChannel channel)
        {
            var savedOffset = buffer.Offset;
            if (buffer.UnallocatedBytes() < 1000)
            {
                Buffer.BlockCopy(buffer.Buffer, buffer.Offset, buffer.Buffer, buffer.StartOffset, buffer.BytesLeft());
                savedOffset = buffer.StartOffset + buffer.BytesLeft();
            }

            var existingLen = buffer.BytesLeft();
            buffer.Offset = buffer.StartOffset+ existingLen;
            await channel.ReceiveAsync(buffer);
            buffer.Offset = savedOffset;
        }
    }
}
