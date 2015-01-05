using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    /// Server for the MicroMsg protocol
    /// </summary>
    public class MicroMessageTcpListener : ChannelTcpListener
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroMessageTcpListener"/> class.
        /// </summary>
        public MicroMessageTcpListener()
            : this(new ChannelTcpListenerConfiguration(() => new MicroMessageDecoder(new DataContractMessageSerializer()), () => new MicroMessageEncoder(new DataContractMessageSerializer())))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MicroMessageTcpListener"/> class.
        /// </summary>
        /// <param name="configuration">Configuration to use.</param>
        public MicroMessageTcpListener(ChannelTcpListenerConfiguration configuration)
            : base(configuration)
        {
        }

    }
}