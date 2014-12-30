
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Is final frame or not
    /// <seealso cref="http://tools.ietf.org/html/rfc6455#section-5.2"/>
    /// </summary>
    public enum WebSocketFin : byte
    {
        /// <summary>
        /// There are more fragments
        /// </summary>
        More = 0x0,

        /// <summary>
        /// This is the final frame
        /// </summary>
        Final = 0x1,
    }
}
