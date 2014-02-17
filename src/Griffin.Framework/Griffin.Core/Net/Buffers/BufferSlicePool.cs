using System.Collections.Concurrent;

namespace Griffin.Net.Buffers
{
    public class BufferSlicePool : IBufferSlicePool
    {
        private byte[] _buffer;
        private int _bufferSize;
        ConcurrentStack<IBufferSlice> _slices = new ConcurrentStack<IBufferSlice>();

        public BufferSlicePool(int bufferSize, int numberOfBuffers)
        {
            _buffer = new byte[bufferSize*numberOfBuffers];
            _bufferSize = bufferSize;
            int offset = 0;
            for (int i = 0; i < numberOfBuffers; i++)
            {
                _slices.Push(new BufferSlice(_buffer, offset, bufferSize));
                offset += bufferSize;
            }    
        }

        public IBufferSlice Pop()

        {
            IBufferSlice pop;
            if (!_slices.TryPop(out pop))
                throw new PoolEmptyException("Out of buffers. You are either not releasing used buffers or have allocated less number of buffers than allowed number of connected clients.");

            return pop;
        }

        public void Push(IBufferSlice bufferSlice)
        {
            _slices.Push(bufferSlice);
        }
    }
}
