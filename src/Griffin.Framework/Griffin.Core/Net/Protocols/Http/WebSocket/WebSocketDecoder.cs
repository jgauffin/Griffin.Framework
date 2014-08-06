using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketDecoder : IMessageDecoder
    {
        /// <summary>
        ///     A message have been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived { get; set; }

        /// <summary>
        ///     Clear state to allow this decoder to be reused.
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
