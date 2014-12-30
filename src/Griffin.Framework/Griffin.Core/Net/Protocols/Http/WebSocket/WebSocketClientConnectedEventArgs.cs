using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket Connected Event which includes the handshake request and response
    /// </summary>
    public class WebSocketClientConnectedEventArgs : EventArgs
    {

        public WebSocketClientConnectedEventArgs(ITcpChannel channel, IHttpRequest request, IHttpResponse response)
        {
            Channel = channel;
            Request = request;
            Response = response;
        }

        /// <summary>
        /// Channel for the connected client
        /// </summary>
        public ITcpChannel Channel { get; private set; }

        /// <summary>
        /// WebSocket handshake request
        /// </summary>
        public IHttpRequest Request { get; private set; }

        /// <summary>
        /// WebSocket handshake response
        /// </summary>
        public IHttpResponse Response { get; private set; }

    }
}
