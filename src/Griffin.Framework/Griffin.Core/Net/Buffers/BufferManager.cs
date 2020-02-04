using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Griffin.Net.Buffers
{
    /// <summary>
    /// Used to reduce memory fragmentation 
    /// </summary>
    public class BufferManager
    {
        private readonly ConcurrentQueue<IBufferSegment> _buffers = new ConcurrentQueue<IBufferSegment>();
        private readonly byte[] _buffer;
        private int _handedOutBytes;

        /// <summary>
        /// Create a new instance of <see cref="BufferManager"/>.
        /// </summary>
        /// <param name="initialAmountOfBuffers">Amount of buffers to preallocate.</param>
        /// <param name="bufferSize">Capacity of each buffer.</param>
        public BufferManager(int initialAmountOfBuffers, int bufferSize)
        {
            BufferSize = bufferSize;
            PainThreshold = 1000000000;
            _buffer = new byte[initialAmountOfBuffers * bufferSize];
            for (int index = 0; index < initialAmountOfBuffers; index++)
            {
                Enqueue(new PooledBufferSegment(this, _buffer, index * bufferSize, bufferSize));
            }

            //Reset since Enqueue decreases it.
            _handedOutBytes = 0;
        }

        /// <summary>
        /// Create a new instance of <see cref="BufferManager"/>.
        /// </summary>
        /// <param name="initialAmountOfBuffers">Amount of buffers to preallocate.</param>
        public BufferManager(int initialAmountOfBuffers)
            : this(initialAmountOfBuffers, 8192)
        {
        }

        /// <summary>
        /// Size of a buffer, 8192 bytes per default.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Amount of bytes that can be handed out before we should throw an exception to indicate memory leakage.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default is 1GB.
        /// </para>
        /// </remarks>
        public int PainThreshold { get; set; }

        /// <summary>
        /// Dequeue a buffer.
        /// </summary>
        /// <returns>Will create a new buffer if the pool is empty</returns>
        public IBufferSegment Dequeue()
        {
            var buffer = _buffers.TryDequeue(out var segment)
                ? segment
                : new PooledBufferSegment(this, BufferSize);

            var value = Interlocked.Add(ref _handedOutBytes, buffer.Capacity);
            if (value > PainThreshold)
                throw new PoolEmptyException("We've handed out a total of " + _handedOutBytes +
                                                    " bytes. That's not very good. It's in fact a pain. Try to discover which part is not handing back buffers.");
            return buffer;
        }

        internal void Enqueue(IBufferSegment bufferSegment)
        {
            Interlocked.Add(ref _handedOutBytes, -bufferSegment.Capacity);
            ((PooledBufferSegment)bufferSegment).Reset();
            _buffers.Enqueue(bufferSegment);
        }
    }
}