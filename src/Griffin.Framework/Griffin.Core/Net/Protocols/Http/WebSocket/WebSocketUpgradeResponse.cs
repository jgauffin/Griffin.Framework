using System.Net;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketUpgradeResponse : HttpResponseBase
    {
        public WebSocketUpgradeResponse(string webSocketKey)
            : base(HttpStatusCode.SwitchingProtocols, "Switching Protocols", "HTTP/1.1")
        {
            Headers["Upgrade"] = "websocket";
            Headers["Connection"] = "Upgrade";
            Headers["Sec-WebSocket-Accept"] = WebSocketUtils.HashWebSocketKey(webSocketKey);
        }
    }
}
