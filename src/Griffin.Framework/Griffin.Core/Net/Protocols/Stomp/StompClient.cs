using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp
{
    /// <summary>
    /// Used to talk with a STOMP server.
    /// </summary>
    /// <remarks>
    /// This client can only parse STOMP frames, it doesn't know what they mean.
    /// </remarks>
    public class StompClientLight : ChannelTcpClient
    {
        public StompClientLight(IMessageSerializer serializer)
            : base(new MicroMessageEncoder(serializer), new MicroMessageDecoder(serializer))
        {
            
        }
    }
}
