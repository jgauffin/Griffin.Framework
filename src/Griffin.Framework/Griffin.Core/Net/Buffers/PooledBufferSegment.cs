using System;

namespace Griffin.Net.Buffers
{
    /// <summary>
    ///     Wraps a <c>byte[]</c> buffer which was allocated by a buffer pool.
    /// </summary>
    public class PooledBufferSegment : IBufferSegment, IPooledObject
    {
        private readonly BufferManager _pool;
        private int _count;
        private int _offset;
        private bool _returned;

        /// <summary>
        ///     Create a new instance of <see cref="PooledBufferSegment" />
        /// </summary>
        /// <param name="pool">Pool that the buffer should be returned to</param>
        /// <param name="capacity">Number of bytes that should be allocated</param>
        public PooledBufferSegment(BufferManager pool, int capacity)
        {
            Capacity = capacity;
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            StartOffset = Offset = Count = 0;
            Buffer = new byte[capacity];
        }

        /// <summary>
        ///     Create a new instance of <see cref="PooledBufferSegment" />
        /// </summary>
        /// <param name="pool">Pool that the buffer should be returned to</param>
        /// <param name="buffer">Buffer that this segment is aprt of</param>
        /// <param name="offset">Where this segment starts</param>
        /// <param name="capacity">Number of bytes that was allocated for this segment</param>
        public PooledBufferSegment(BufferManager pool, byte[] buffer, int offset, int capacity)
        {
            Capacity = capacity;
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            StartOffset = offset;
            Offset = offset;
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
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
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"Cannot be less than StartOffset which is {StartOffset}.");

                //same pos is OK, it just means that the buffer is full.
                if (value > StartOffset + Capacity)
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"Cannot be larger than StartOffset+Capacity which is {StartOffset + Capacity}.");

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
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "Cannot be larger than the Capacity which is " + Capacity);
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
        ///     Return the buffer to the pool (i.e. it can now be reused somewhere else in the code).
        /// </summary>
        public void ReturnToPool()
        {
            if (_returned)
                throw new InvalidOperationException("Already returned to pool.");
            _pool.Enqueue(this);
            _returned = true;
        }

        /// <summary>
        ///     Reset buffer, i.e. move <c>Offset</c> to <c>StartOffset</c> and set <c>Count</c> to 0.e
        /// </summary>
        public void Reset()
        {
            Offset = StartOffset;
            Count = 0;
            _returned = false;
        }
    }
}