using System.Net;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Different types of Websocket responses.
    /// </summary>
    public class WebSocketResponses
    {
        /// <summary>
        ///     Used to confirm that we can switch to WEBSOCKETs from regular HTTP Requests.
        /// </summary>
        /// <param name="webSocketKey">Key from the HTTP request.</param>
        public static HttpResponse Upgrade(string webSocketKey)
        {
            var response = new HttpResponse(HttpStatusCode.SwitchingProtocols, "Switching Protocols", "HTTP/1.1");
            response.Headers["Upgrade"] = "websocket";
            response.Headers["Connection"] = "Upgrade";
            response.Headers["Sec-WebSocket-Accept"] = WebSocketUtils.HashWebSocketKey(webSocketKey);
            return response;
        }
    }
}