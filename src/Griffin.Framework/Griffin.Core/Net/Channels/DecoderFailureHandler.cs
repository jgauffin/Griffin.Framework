using System;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Invoked by <see cref="ITcpChannel"/> if the <see cref="IMessageDecoder"/> failes to parse incoming message.
    /// </summary>
    /// <param name="channel">Channel that the decoder belongs to</param>
    /// <param name="error">Why the decoder failed</param>
    /// <remarks>
    /// <para>
    /// Typically the handler will send an error message and close the connection.
    /// </para>
    /// </remarks>
    public delegate void DecoderFailureHandler(ITcpChannel channel, Exception error);
}