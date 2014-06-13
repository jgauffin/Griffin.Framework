using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg.Server
{
    public class MicroMessageTcpListener : ChannelTcpListener
    {

        /// <summary>
        /// 
        /// </summary>
        public MicroMessageTcpListener()
            : this(new ChannelTcpListenerConfiguration(() => new MicroMessageDecoder(new DataContractMessageSerializer()), () => new MicroMessageEncoder(new DataContractMessageSerializer())))
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public MicroMessageTcpListener(ChannelTcpListenerConfiguration configuration)
            : base(configuration)
        {
        }

    }
}