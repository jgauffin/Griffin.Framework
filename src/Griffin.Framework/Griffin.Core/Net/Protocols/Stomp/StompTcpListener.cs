using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Stomp
{
    /// <summary>
    /// Listens on STOMP messages from a client.
    /// </summary>
    public class StompTcpListener : ChannelTcpListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StompTcpListener"/> class.
        /// </summary>
        public StompTcpListener() : base(new ChannelTcpListenerConfiguration(() => new StompDecoder(), () => new StompEncoder()))
        {
        }

        protected override void OnMessage(ITcpChannel source, object msg)
        {

            base.OnMessage(source, msg);
        }

        protected override ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {

            return base.OnClientConnected(channel);
        }
    }
}
