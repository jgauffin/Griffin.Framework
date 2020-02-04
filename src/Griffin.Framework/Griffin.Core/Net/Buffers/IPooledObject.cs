namespace Griffin.Net.Buffers
{
    /// <summary>
    /// For pooled objects
    /// </summary>
    public interface IPooledObject
    {
        /// <summary>
        ///     Release buffer back to the pool (typically done by the socket once it's sent successfully).
        /// </summary>
        void ReturnToPool();
    }
}
