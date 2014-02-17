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

        public SocketAsyncEventArgsWrapper(SocketAsyncEventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");

            _args = args;
            Capacity = args.Count;
            BaseOffset = args.Offset;
        }

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
        /// Assign a buffer.
        /// </summary>
        /// <param name="buffer">Buffer to use</param>
        /// <param name="offset">Offset to start next operation on</param>
        /// <param name="count">Amount of bytes to process.</param>
        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            BaseOffset = offset;
            _args.SetBuffer(buffer, offset, count);
        }
    }
}