using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket response
    /// </summary>
    public class WebSocketResponse : WebSocketMessage
    {
        internal WebSocketResponse(WebSocketFrame frame)
            : base(frame.Opcode, frame.Payload)
        {
        }
    }
}
