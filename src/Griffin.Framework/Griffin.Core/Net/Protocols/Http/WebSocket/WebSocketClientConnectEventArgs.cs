using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket Connect Event which includes the handshake request
    /// </summary>
    public class WebSocketClientConnectEventArgs : ClientConnectedEventArgs
    {

        public WebSocketClientConnectEventArgs(ITcpChannel channel, IHttpRequest request)
            : base(channel)
        {
            Request = request;
        }

        /// <summary>
        /// WebSocket handshake request
        /// </summary>
        public IHttpRequest Request { get; private set; }

    }
}
