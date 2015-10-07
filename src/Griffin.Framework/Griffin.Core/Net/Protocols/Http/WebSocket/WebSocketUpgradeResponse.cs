using System.Net;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Used to confirm that we can switch to WEBSOCKETs from regular HTTP Requests.
    /// </summary>
    public class WebSocketUpgradeResponse : HttpResponse
    {
        /// <summary>
        /// Create a new instance of <see cref="WebSocketUpgradeResponse"/>
        /// </summary>
        /// <param name="webSocketKey">Key from the HTTP request.</param>
        public WebSocketUpgradeResponse(string webSocketKey)
            : base(HttpStatusCode.SwitchingProtocols, "Switching Protocols", "HTTP/1.1")
        {
            Headers["Upgrade"] = "websocket";
            Headers["Connection"] = "Upgrade";
            Headers["Sec-WebSocket-Accept"] = WebSocketUtils.HashWebSocketKey(webSocketKey);
        }
    }
}
