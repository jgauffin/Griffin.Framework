
namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketUpgradeRequest : HttpRequestBase
    {
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
