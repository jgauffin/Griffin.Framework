using System;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// <see cref="ITcpChannel"/> got disconnected
    /// </summary>
    /// <param name="channel">Channel which got disconnected</param>
    /// <param name="exception">Exception  (<c>SocketException</c> for TCP errors)</param>
    /// <seealso cref="ITcpChannel"/>
    public delegate void DisconnectHandler(ITcpChannel channel, Exception exception);
}