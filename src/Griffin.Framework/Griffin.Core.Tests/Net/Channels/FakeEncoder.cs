using System;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Core.Tests.Net.Channels
{
    public class FakeEncoder : IMessageEncoder
    {
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }

        public FakeEncoder()
        {
            Buffer = new byte[65535];
            Offset = 0;
            Count = 10;

        }
        /// <summary>
        /// Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>Can be used to prepare the next message. for instance serialize it etc.
        /// 
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message)
        {
        }

        /// <summary>
        /// Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">Do note that there are not buffer attached to the structure, you have to assign one yourself using <see cref="ISocketBuffer.SetBuffer"/>. This choice was made
        /// to prevent unnecessary copy operations.</param>
        public void Send(ISocketBuffer buffer)
        {
            buffer.SetBuffer(Buffer, Offset, Count);
        }

        /// <summary>
        /// The previous <see cref="IMessageEncoder.Send"/> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred)
        {
            BytesTransferred = bytesTransferred;
            return true;
        }

        public int BytesTransferred { get; set; }

        /// <summary>
        /// Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            
        }
    }
}