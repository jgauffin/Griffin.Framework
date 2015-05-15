
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Is final frame or not
    /// </summary>
    /// <remarks>
    /// <para>The specification for this flag can be found in http://tools.ietf.org/html/rfc6455#section-5.2.
    /// </para>
    /// </remarks>
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
