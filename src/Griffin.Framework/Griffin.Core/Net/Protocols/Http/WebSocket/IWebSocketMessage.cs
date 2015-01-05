using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Interface for WebSocket messages
    /// </summary>
    public interface IWebSocketMessage
    {
        /// <summary>
        /// Type of message
        /// </summary>
        WebSocketOpcode Opcode { get; set; }

        /// <summary>
        /// Message payload
        /// </summary>
        Stream Payload { get; set; }
    }
}
