using System;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Net
{
    /// <summary>
    /// Used to create secure channels for our library.
    /// </summary>
    public  class SecureTcpChannelFactory : ITcpChannelFactory
    {
        private readonly ISslStreamBuilder _sslStreamBuilder;

        public SecureTcpChannelFactory(ISslStreamBuilder sslStreamBuilder)
        {
            if (sslStreamBuilder == null) throw new ArgumentNullException("sslStreamBuilder");

            _sslStreamBuilder = sslStreamBuilder;
        }

        /// <summary>
        /// Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        public ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder)
        {
            return new SecureTcpChannel(readBuffer, encoder, decoder, _sslStreamBuilder);
        }
    }
}