
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Is data masked or not
    /// </summary>
    /// <remarks>
    /// <para>
    /// The specification for this mask can be found in http://tools.ietf.org/html/rfc6455#section-5.2.
    /// </para>
    /// </remarks>
    public enum WebSocketMask : byte
    {
        /// <summary>
        /// Data is not masked
        /// </summary>
        Unmask = 0x0,

        /// <summary>
        /// Data is masked
        /// </summary>
        Mask = 0x1,
    }
}
