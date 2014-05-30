using System;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to create channels.
    /// </summary>
    /// <remarks>
    /// <para>Can be used to adjust how all lower level functions should work, like protecting everything with SSL</para>
    /// <para></para>
    /// </remarks>
    public interface ITcpChannelFactory
    {
        /// <summary>
        /// Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder);

        /// <summary>
        /// create a new queue which are used to store outbound messages in the created channel.
        /// </summary>
        /// <returns>Factory method</returns>
        Func<IMessageQueue> OutboundMessageQueueFactory { get; set; }
    }
}