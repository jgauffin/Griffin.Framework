namespace Griffin.Net.Buffers
{
    /// <summary>
    /// Represents a part of a larger byte buffer.
    /// </summary>
    public interface IBufferSlicePool
    {
        /// <summary>
        /// Pop a new slice
        /// </summary>
        /// <returns>New slice</returns>
        /// <exception cref="PoolEmptyException">There are no more free slices in the pool.</exception>
        IBufferSlice Pop();

        /// <summary>
        /// Return a slice to the pool
        /// </summary>
        /// <param name="bufferSlice">Slice retrieved using <see cref="Pop"/>.</param>
        void Push(IBufferSlice bufferSlice);
    }
}