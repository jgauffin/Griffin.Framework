using System;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     Creates a <see cref="TcpChannel" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Allows you to provide your own custom channels to be able to control the IO operations that this library uses.
    ///     </para>
    /// </remarks>
    public class TcpChannelFactory : ITcpChannelFactory
    {
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
            if (OutboundMessageQueueFactory != null)
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