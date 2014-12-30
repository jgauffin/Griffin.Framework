
namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Websocket extension switch
    /// <seealso cref="http://tools.ietf.org/html/rfc6455#section-5.2"/>
    /// <seealso cref="http://tools.ietf.org/html/rfc6455#section-5.8"/>
    /// </summary>
    public enum WebSocketRsv : byte
    {
        Off = 0x0,
        On = 0x1,
    }
}
