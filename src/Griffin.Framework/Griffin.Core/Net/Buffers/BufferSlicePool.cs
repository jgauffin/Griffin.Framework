using System;
using System.Collections.Concurrent;

namespace Griffin.Net.Buffers
{
    /// <summary>
    /// Creates a large buffer and slices it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Thread safe, can be used to reuse slices.
    /// </para>
    /// </remarks>
    public class BufferSlicePool : IBufferSlicePool
    {
        private byte[] _buffer;
        private int _sliceSize;
        ConcurrentStack<IBufferSlice> _slices = new ConcurrentStack<IBufferSlice>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferSlicePool"/> class.
        /// </summary>
        /// <param name="sliceSize">How large each slice should be.</param>
        /// <param name="numberOfBuffers">The number of slices.</param>
        public BufferSlicePool(int sliceSize, int numberOfBuffers)
        {
            _buffer = new byte[sliceSize*numberOfBuffers];
            _sliceSize = sliceSize;
            int offset = 0;
            for (int i = 0; i < numberOfBuffers; i++)
            {
                _slices.Push(new BufferSlice(_buffer, offset, sliceSize));
                offset += sliceSize;
            }    
        }

        /// <summary>
        /// Get a new slice
        /// </summary>
        /// <returns>Slice</returns>
        /// <exception cref="PoolEmptyException">Out of buffers. You are either not releasing used buffers or have allocated fewer buffers than allowed number of connected clients.</exception>
        public IBufferSlice Pop()

        {
            IBufferSlice pop;
            if (!_slices.TryPop(out pop))
                throw new PoolEmptyException("Out of buffers. You are either not releasing used buffers or have allocated fewer buffers than allowed number of connected clients.");

            return pop;
        }

        /// <summary>
        /// Enqueue a slice to be able to re-use it later
        /// </summary>
        /// <param name="bufferSlice">Slice to append</param>
        /// <exception cref="System.ArgumentNullException">bufferSlice</exception>
        public void Push(IBufferSlice bufferSlice)
        {
            if (bufferSlice == null) throw new ArgumentNullException("bufferSlice");
            _slices.Push(bufferSlice);
        }
    }
}
