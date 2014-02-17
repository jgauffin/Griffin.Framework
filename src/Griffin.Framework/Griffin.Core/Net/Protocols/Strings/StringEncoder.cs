using System;
using System.Net.Sockets;
using System.Text;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Strings
{
    public class StringEncoder : IMessageEncoder
    {
        private byte[] _buffer = new byte[65535];
        private int _bytesLeft = 0;
        private int _offset;
        /// <summary>
        ///     Prepare the encoder so that the specified object can be encoded next.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message)
        {
            var len = Encoding.UTF8.GetByteCount(message.ToString());
            BitConverter2.GetBytes(len, _buffer, 0);
            Encoding.UTF8.GetBytes(message.ToString(), 0, message.ToString().Length, _buffer, 4);
            _bytesLeft = len + 4;
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        /// <remarks>
        ///     The <c>buffer</c> variable is typically a wrapper around <see cref="SocketAsyncEventArgs" />, but may be something
        ///     else if required.
        /// </remarks>
        public void Send(ISocketBuffer buffer)
        {
            buffer.SetBuffer(_buffer, _offset, _bytesLeft);

        }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred)
        {
            _offset += bytesTransferred;
            _bytesLeft -= bytesTransferred;
            return _bytesLeft <= 0;
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            _bytesLeft = 0;
            _offset = 0;
        }
    }
}