using System;
using System.Net.Sockets;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// The real implementation which uses <c>SocketAsyncEventArgs</c> internally.
    /// </summary>
    public class SocketAsyncEventArgsWrapper : ISocketBuffer
    {
        private readonly SocketAsyncEventArgs _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketAsyncEventArgsWrapper"/> class.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ArgumentNullException">args</exception>
        public SocketAsyncEventArgsWrapper(SocketAsyncEventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");

            _args = args;
            Capacity = args.Count;
            BaseOffset = args.Offset;
            
        }

        /// <summary>
        ///     an object which can be used by you to keep track of what's being sent and received.
        /// </summary>
        public object UserToken { get { return _args.UserToken; } set { _args.UserToken = value; } }

        /// <summary>
        /// Amount of bytes which was transferred in the last I/O operation
        /// </summary>
        public int BytesTransferred
        {
            get { return _args.BytesTransferred; }
        }

        /// <summary>
        /// Amount of bytes in our buffer.
        /// </summary>
        public int Count
        {
            get { return _args.Count; }
        }

        /// <summary>
        /// Amount of bytes which we can use in the buffer.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Buffer to use (or rather a slice of it)
        /// </summary>
        public byte[] Buffer
        {
            get { return _args.Buffer; }
        }

        /// <summary>
        /// Where our slice starts in the <see cref="Buffer"/>.
        /// </summary>
        public int BaseOffset { get; private set; }

        /// <summary>
        /// Current offset in the buffer.
        /// </summary>
        public int Offset
        {
            get { return _args.Offset; }
        }

        /// <summary>
        /// Set the bytes which currently should be transferred in the next I/O operation (or the bytes which was just received)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void SetBuffer(int offset, int count)
        {
            _args.SetBuffer(offset, count);
        }

        /// <summary>
        /// Assign a buffer to the structure
        /// </summary>
        /// <param name="buffer">Buffer to use</param>
        /// <param name="offset">Index of first byte to send</param>
        /// <param name="count">Amount of bytes to send</param>
        /// <param name="capacity">Total number of bytes allocated for this slices</param>
        public void SetBuffer(byte[] buffer, int offset, int count, int capacity)
        {
            BaseOffset = offset;
            Capacity = capacity;
            _args.SetBuffer(buffer, offset, count);
        }

        /// <summary>
        /// Assign a buffer to the structure
        /// </summary>
        /// <param name="buffer">Buffer to use</param>
        /// <param name="offset">Index of first byte to send</param>
        /// <param name="count">Amount of bytes to send</param>
        /// <remarks>
        /// Capacity will be set to same as <c>count</c>.
        /// </remarks>
        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            BaseOffset = offset;
            Capacity = count;
            _args.SetBuffer(buffer, offset, count);
        }
    }
}