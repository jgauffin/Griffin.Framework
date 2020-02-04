using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket message
    /// </summary>
    public class WebSocketMessage
    {
        /// <summary>
        /// Creates a new WebSocket message with empty payload. This is useful for control messages such as PING, PONG and CLOSE
        /// </summary>
        /// <param name="opCode">opCode</param>
        public WebSocketMessage(WebSocketOpcode opCode)
            : this(opCode, Stream.Null)
        {
        }

        /// <summary>
        /// Create a new WebSocket message with predefined payload
        /// </summary>
        /// <param name="opCode">opCode</param>
        /// <param name="payload">payload</param>
        public WebSocketMessage(WebSocketOpcode opCode, Stream payload)
        {
            OpCode = opCode;
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        /// <summary>
        /// Kind of web socket message
        /// </summary>
        public WebSocketOpcode OpCode { get; set; }

        /// <summary>
        /// Received message
        /// </summary>
        public Stream Payload { get; set; }
    }
}
