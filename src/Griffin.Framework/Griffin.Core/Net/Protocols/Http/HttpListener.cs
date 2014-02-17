using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http
{
    public class HttpListener : ProtocolTcpListener
    {
        private int _pipelineIndex;

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public HttpListener(ProtocolListenerConfiguration configuration) : base(configuration)
        {
        }

        public HttpListener()
        {
            var config = new ProtocolListenerConfiguration(
                () => new HttpMessageDecoder(),
                () => new HttpMessageEncoder());

            Configure(config);
        }

        protected override ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {
            //GriffinNetworking.cerchannel.
            return base.OnClientConnected(channel);
        }

        protected override void OnMessage(ITcpChannel source, object msg)
        {
            var message = (IHttpMessage) msg;

            // used to be able to send back all
            message.Headers["X-Pipeline-index"] = (_pipelineIndex++).ToString();

            base.OnMessage(source, msg);
        }
    }
}
