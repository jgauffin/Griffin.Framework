using Griffin.Net.Protocols.MicroMsg.Serializers;

namespace Griffin.Net.Protocols.MicroMsg.Server
{
    public class MicroMessageTcpListener : ProtocolTcpListener
    {

        /// <summary>
        /// 
        /// </summary>
        public MicroMessageTcpListener()
            : this(new ProtocolListenerConfiguration(() => new MicroMessageDecoder(new DataContractMessageSerializer()), () => new MicroMessageEncoder(new DataContractMessageSerializer())))
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public MicroMessageTcpListener(ProtocolListenerConfiguration configuration)
            : base(configuration)
        {
        }

    }
}