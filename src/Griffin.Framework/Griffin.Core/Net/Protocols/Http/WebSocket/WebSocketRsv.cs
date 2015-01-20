namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    ///     Websocket extension switch
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The specification for this option can be found in http://tools.ietf.org/html/rfc6455#section-5.2 and
    ///         http://tools.ietf.org/html/rfc6455#section-5.8.
    ///     </para>
    /// </remarks>
    public enum WebSocketRsv : byte
    {
        /// <summary>
        ///     Off
        /// </summary>
        Off = 0x0,

        /// <summary>
        ///     On
        /// </summary>
        On = 0x1,
    }
}
