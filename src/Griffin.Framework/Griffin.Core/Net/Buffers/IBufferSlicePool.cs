namespace Griffin.Net.Buffers
{
    public interface IBufferSlicePool
    {
        IBufferSlice Pop();
        void Push(IBufferSlice bufferSlice);
    }
}