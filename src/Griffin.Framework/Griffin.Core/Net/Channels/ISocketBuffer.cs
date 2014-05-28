namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to wrap <c>SocketAsyncEventArgs</c> to make everything testable (which <c>SocketAsyncEventArgs</c> isn't due to private setters and socket references).
    /// </summary>
    /// <seealso cref="SocketAsyncEventArgsWrapper"/>
    public interface ISocketBuffer
    {
        /// <summary>
        /// Number of bytes which was received or transmitted in the last Socket operation
        /// </summary>
        int BytesTransferred { get; }

        /// <summary>
        /// Amount of bytes to receive or send in the next Socket operation.
        /// </summary>
        /// <seealso cref="Offset"/>
        int Count{ get; }

        /// <summary>
        /// Amount of bytes allocated for this buffer
        /// </summary>
        int Capacity {  get; }

        /// <summary>
        /// Buffer used for transfers
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// Offset in buffer were our allocated part starts
        /// </summary>
        /// <remarks>A buffer can have been divided between many channels. this index tells us where our slice starts.</remarks>
        int BaseOffset { get; }

        /// <summary>
        /// Start offset for the next socket operation. (Typically same as BaseOffset unless this is a continuation of a partial message send).
        /// </summary>
        /// <seeAlso cref="Count"/>
        int Offset { get; }

        /// <summary>
        /// Reuse the previously specified buffer, but change the offset/count of the bytes to send.
        /// </summary>
        /// <param name="offset">Index of first byte to send</param>
        /// <param name="count">Amount of bytes to send</param>
        void SetBuffer(int offset, int count);

        /// <summary>
        /// Assign a buffer to the structure
        /// </summary>
        /// <param name="buffer">Buffer to use</param>
        /// <param name="offset">Index of first byte to send</param>
        /// <param name="count">Amount of bytes to send</param>
        /// <param name="capacity">Total number of bytes allocated for this slices</param>
        void SetBuffer(byte[] buffer, int offset, int count, int capacity);

        /// <summary>
        /// Assign a buffer to the structure
        /// </summary>
        /// <param name="buffer">Buffer to use</param>
        /// <param name="offset">Index of first byte to send</param>
        /// <param name="count">Amount of bytes to send</param>
        /// <remarks>Capacity will be set to same as <c>count</c>.</remarks>
        void SetBuffer(byte[] buffer, int offset, int count);
    }
}
