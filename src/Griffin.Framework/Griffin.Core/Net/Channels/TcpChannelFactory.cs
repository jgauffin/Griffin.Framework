using System;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     Creates a <see cref="TcpChannel" />.
    /// </summary>
    public class TcpChannelFactory : ITcpChannelFactory
    {
        private readonly Func<IMessageQueue> _outboundMessageQueueFactory;

        public TcpChannelFactory()
        {
            _outboundMessageQueueFactory = () => new MessageQueue();
        }

        /// <summary>
        ///     Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        public ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder)
        {
            var channel = new TcpChannel(readBuffer, encoder, decoder);
            if (_outboundMessageQueueFactory != null)
                channel.OutboundMessageQueue = OutboundMessageQueueFactory();
            return channel;
        }

        /// <summary>
        ///     create a new queue which are used to store outbound messages in the created channel.
        /// </summary>
        /// <returns>Factory method</returns>
        public Func<IMessageQueue> OutboundMessageQueueFactory { get; set; }
    }
}