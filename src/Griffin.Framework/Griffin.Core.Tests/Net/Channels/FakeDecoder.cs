using System;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Core.Tests.Net.Channels
{
    public class FakeDecoder : IMessageDecoder
    {
        /// <summary>
        /// We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            
        }

        public void Clear()
        {
            
        }

        /// <summary>
        /// A message has been received.
        /// </summary>
        /// <remarks>
        /// Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived { get; set; }
    }
}