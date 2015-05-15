using System;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to create secure channels for our library.
    /// </summary>
    public  class SecureTcpChannelFactory : ITcpChannelFactory
    {
        private readonly ISslStreamBuilder _sslStreamBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureTcpChannelFactory"/> class.
        /// </summary>
        /// <param name="sslStreamBuilder">The SSL stream builder.</param>
        /// <exception cref="System.ArgumentNullException">sslStreamBuilder</exception>
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
            var channel= new SecureTcpChannel(readBuffer, encoder, decoder, _sslStreamBuilder);
            if (OutboundMessageQueueFactory != null)
                channel.OutboundMessageQueue = OutboundMessageQueueFactory();

            return channel;
        }

        /// <summary>
        /// Create a new queue which is used to store outbound messages in the created channel.
        /// </summary>
        /// <returns>Factory method</returns>
        public Func<IMessageQueue> OutboundMessageQueueFactory { get; set; }
    }
}