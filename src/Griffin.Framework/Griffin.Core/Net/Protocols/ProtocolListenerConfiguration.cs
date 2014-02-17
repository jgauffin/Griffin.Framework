using System;
using Griffin.Net.Buffers;

namespace Griffin.Net.Protocols
{
    public class ProtocolListenerConfiguration
    {
        public ProtocolListenerConfiguration(Func<IMessageDecoder> decoderFactory, Func<IMessageEncoder> encoderFactory)
        {
            if (decoderFactory == null) throw new ArgumentNullException("decoderFactory");
            if (encoderFactory == null) throw new ArgumentNullException("encoderFactory");

            DecoderFactory = decoderFactory;
            EncoderFactory = encoderFactory;
            BufferPool = new BufferSlicePool(65535, 100);
        }

        /// <summary>
        /// Factory used to produce a decoder for every connected client
        /// </summary>
        public Func<IMessageDecoder> DecoderFactory { get; set; }

        /// <summary>
        /// Factory used to produce an encoder for every connected client
        /// </summary>
        public Func<IMessageEncoder> EncoderFactory { get; set; }

        /// <summary>
        /// Pool used to allocate buffers for every client
        /// </summary>
        /// <remarks>
        /// Each client requires one buffer (for receiving).
        /// </remarks>
        /// <value>
        /// 100 buffers of size 65535 bytes are allocated per default.
        /// </value>
        public IBufferSlicePool BufferPool { get; set; }

       
    }
}
