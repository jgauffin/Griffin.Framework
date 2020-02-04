using System;

namespace Griffin.Net.Buffers
{
    internal class StandAloneBuffer : IBufferSegment
    {
        private int _count;
        private int _offset;

        /// <summary>
        ///     Create a new instance of <see cref="PooledBufferSegment" />
        /// </summary>
        /// <param name="capacity">Number of bytes that should be allocated</param>
        public StandAloneBuffer(int capacity)
        {
            Capacity = capacity;
            StartOffset = Offset = Count = 0;
            Buffer = new byte[capacity];
        }

        /// <summary>
        ///     Create a new instance of <see cref="PooledBufferSegment" />
        /// </summary>
        /// <param name="buffer">Buffer that this segment is part of</param>
        /// <param name="offset">Where to start read/write.</param>
        /// <param name="count">Amount of bytes available for read/write</param>
        public StandAloneBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < offset)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Offset is beyond the end of the allocated buffer");
            if (buffer.Length < count)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Capacity is larger than the allocated buffer");
            if (buffer.Length < offset+count)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Offset + Capacity is beyond the end of the allocated buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            Capacity = buffer.Length;
            StartOffset = 0;
            Offset = offset;
            Buffer = buffer;
        }

        /// <summary>
        ///     Set offset in buffer, i.e where the next operation on the buffer should take place.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The offset can only be set in the range from <see cref="StartOffset" /> to <c>StartOffset + Capacity</c>.
        ///     </para>
        /// </remarks>
        public int Offset
        {
            get => _offset;
            set
            {
                if (value < StartOffset)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"Cannot be less than StartOffset which is {StartOffset}.");
                }

                //same pos is OK, it just means that the buffer is full.
                if (value > StartOffset + Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"Cannot be larger than StartOffset+Capacity which is {StartOffset + Capacity}.");
                }

                _offset = value;
            }
        }

        /// <summary>
        ///     Number of bytes allocated for this buffer.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        ///     First position in the buffer that was allocated for this segment.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        ///     Number of bytes that our segment contains.
        /// </summary>
        public int Count
        {
            get => _count;
            set
            {
                if (value > Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "Cannot be larger than the Capacity which is " + Capacity);
                }

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Being a bit negative, aren't we?");

                _count = value;
            }
        }

        /// <summary>
        ///     Actual buffer
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        ///     Check if the buffer will overflow if the specified amount of bytes would be added.
        /// </summary>
        /// <param name="countToAdd">Bytes that want's to be added.</param>
        /// <returns><c>true</c> if the given amount is larger than the remaining capacity.</returns>
        bool IBufferSegment.WillOverflow(int countToAdd)
        {
            var remaining = Capacity - Count;
            return countToAdd > remaining;
        }

        /// <summary>
        ///     Reset buffer, i.e. move <c>Offset</c> to <c>StartOffset</c> and set <c>Count</c> to 0.e
        /// </summary>
        public void Reset()
        {
            Offset = StartOffset;
            Count = 0;
        }
    }
}