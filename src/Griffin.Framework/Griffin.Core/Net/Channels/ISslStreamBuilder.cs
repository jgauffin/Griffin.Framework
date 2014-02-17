using System.Net.Security;
using System.Net.Sockets;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to build SSL streams (i.e. create and do the initial handshake)
    /// </summary>
    public interface ISslStreamBuilder
    {
        /// <summary>
        /// Build a new SSL steam.
        /// </summary>
        /// <param name="channel">Channel which will use the stream</param>
        /// <param name="socket">Socket to wrap</param>
        /// <returns>Stream which is ready to be used (must have been validated)</returns>
        SslStream Build(ITcpChannel channel, Socket socket);
    }
}