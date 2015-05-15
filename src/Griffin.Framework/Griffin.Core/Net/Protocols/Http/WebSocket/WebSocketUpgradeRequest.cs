
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// HTTP request used to indicate that we want to use the WEBSOCKET protocol.
    /// </summary>
    public class WebSocketUpgradeRequest : HttpRequestBase
    {
        /// <summary>
        /// Create a new instance of <see cref="WebSocketUpgradeRequest"/>.
        /// </summary>
        public WebSocketUpgradeRequest()
            : base("GET", "/", "HTTP/1.1")
        {
            Headers["Upgrade"] = "websocket";
            Headers["Connection"] = "Upgrade";
            Headers["Sec-WebSocket-Key"] = WebSocketUtils.CreateWebSocketKey();
            Headers["Sec-WebSocket-Version"] = "13";
        }
    }
}
