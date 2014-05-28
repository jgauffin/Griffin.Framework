using System;

namespace Griffin.Net.Buffers
{
    /// <summary>
    ///     Used to slice a larger buffer into smaller chunks.
    /// </summary>
    public class BufferSlice : IBufferSlice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BufferSlice" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">Start offset in buffer.</param>
        /// <param name="count">Number of bytes allocated for this slice..</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset;Offset+Count must be less than the buffer length.</exception>
        public BufferSlice(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("offset", offset,
                    "Offset+Count must be less than the buffer length.");

            Capacity = count;
            Offset = offset;
            Buffer = buffer;
        }

        /// <summary>
        ///     Where this slice starts
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        ///     AMount of bytes allocated for this slice.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        ///     Buffer that this slice is in.
        /// </summary>
        public byte[] Buffer { get; private set; }
    }
}