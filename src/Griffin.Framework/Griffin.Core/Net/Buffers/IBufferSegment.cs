namespace Griffin.Net.Buffers
{
    /// <summary>
    ///     Represents a buffer. Can be a part of a quite large buffer or a specifically allocated buffer.
    /// </summary>
    public interface IBufferSegment
    {
        /// <summary>
        ///     The entire buffer, writes/reads should be made from <see cref="StartOffset" /> to <c>StartOffset+Capacity</c>.
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        ///     Number of bytes allocated to our slice.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        ///     Number of bytes that our slice currently holds
        /// </summary>
        int Count { get; set; }

        /// <summary>
        ///     Current position (offset is from the entire buffer, i.e. in the range from <c>StartOffset</c> to
        ///     <c>StartOffset+Capacity</c>).
        /// </summary>
        int Offset { get; set; }

        /// <summary>
        ///     Where this allocated slice starts.
        /// </summary>
        int StartOffset { get; }

        /// <summary>
        /// Check if the buffer will overflow if the specified amount of bytes would be added.
        /// </summary>
        /// <param name="countToAdd">Bytes that want's to be added.</param>
        /// <returns><c>true</c> if the given amount is larger than the remaning capacity.</returns>
        bool WillOverflow(int countToAdd);
    }
}