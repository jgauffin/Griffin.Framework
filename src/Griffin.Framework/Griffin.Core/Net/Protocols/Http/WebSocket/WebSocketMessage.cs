using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket message
    /// </summary>
    public class WebSocketMessage : IWebSocketMessage
    {
        /// <summary>
        /// Creates a new WebSocket message with empty payload. This is useful for control messages such as PING, PONG and CLOSE
        /// </summary>
        /// <param name="opcode">opcode</param>
        public WebSocketMessage(WebSocketOpcode opcode)
            : this(opcode, Stream.Null)
        {
        }

        /// <summary>
        /// Create a new WebSocket message with predefined payload
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="payload">payload</param>
        public WebSocketMessage(WebSocketOpcode opcode, Stream payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");

            Opcode = opcode;
            Payload = payload;
        }

        /// <summary>
        /// Kind of web socket message
        /// </summary>
        public WebSocketOpcode Opcode { get; set; }

        /// <summary>
        /// Received message
        /// </summary>
        public Stream Payload { get; set; }
    }
}
