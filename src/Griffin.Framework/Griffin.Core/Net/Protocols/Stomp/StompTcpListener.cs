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

        /// <summary>
        /// Receive a new message from the specified client
        /// </summary>
        /// <param name="source">Channel for the client</param>
        /// <param name="msg">Message (as decoded by the specified <see cref="IMessageDecoder" />).</param>
        protected override void OnMessage(ITcpChannel source, object msg)
        {

            base.OnMessage(source, msg);
        }

        /// <summary>
        /// A client has connected (nothing has been sent or received yet)
        /// </summary>
        /// <param name="channel">Channel which we created for the remote socket.</param>
        /// <returns></returns>
        protected override ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {

            return base.OnClientConnected(channel);
        }
    }
}
