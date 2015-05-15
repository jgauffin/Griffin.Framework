
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Type of a websocket frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The specification for this op code can be found in http://tools.ietf.org/html/rfc6455#section-5.2 and http://tools.ietf.org/html/rfc6455#section-11.8.
    /// </para>
    /// </remarks>
    public enum WebSocketOpcode : byte
    {
        /// <summary>
        /// Equivalent to numeric value 0.
        /// Indicates a continuation frame.
        /// </summary>
        Continuation = 0x0,

        /// <summary>
        /// Equivalent to numeric value 1.
        /// Indicates a text frame.
        /// </summary>
        Text = 0x1,

        /// <summary>
        /// Equivalent to numeric value 2.
        /// Indicates a binary frame.
        /// </summary>
        Binary = 0x2,

        /// <summary>
        /// Equivalent to numeric value 8.
        /// Indicates a connection close frame.
        /// </summary>
        Close = 0x8,

        /// <summary>
        /// Equivalent to numeric value 9.
        /// Indicates a ping frame.
        /// </summary>
        Ping = 0x9,

        /// <summary>
        /// Equivalent to numeric value 10.
        /// Indicates a pong frame.
        /// </summary>
        Pong = 0xa,
    }
}
