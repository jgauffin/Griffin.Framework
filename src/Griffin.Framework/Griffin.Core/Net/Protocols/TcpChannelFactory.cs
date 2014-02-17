using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    /// Creates a <see cref="TcpChannel"/>.
    /// </summary>
    public class TcpChannelFactory : ITcpChannelFactory
    {
        /// <summary>
        /// Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        public ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder)
        {
            return new TcpChannel(readBuffer, encoder, decoder);
        }
    }
}