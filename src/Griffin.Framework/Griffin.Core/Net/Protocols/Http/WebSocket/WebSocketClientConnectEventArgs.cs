using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket Connect Event which includes the handshake request
    /// </summary>
    public class WebSocketClientConnectEventArgs : ClientConnectedEventArgs
    {
        /// <summary>
        /// Create a new instance of <see cref="WebSocketClientConnectEventArgs"/>
        /// </summary>
        /// <param name="channel">Channel that connected</param>
        /// <param name="request">Request that we received</param>
        public WebSocketClientConnectEventArgs(ITcpChannel channel, IHttpRequest request)
            : base(channel)
        {
            if (request == null) throw new ArgumentNullException("request");
            Request = request;
        }

        /// <summary>
        /// WebSocket handshake request
        /// </summary>
        public IHttpRequest Request { get; private set; }

    }
}
