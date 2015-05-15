using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket Connected Event which includes the handshake request and response
    /// </summary>
    public class WebSocketClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new isntance of <see cref="WebSocketClientConnectedEventArgs"/>
        /// </summary>
        /// <param name="channel">Channel used for transfers</param>
        /// <param name="request">Request (should contain the upgrade request)</param>
        /// <param name="response">Response (should include the upgrade confirmation)</param>
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
