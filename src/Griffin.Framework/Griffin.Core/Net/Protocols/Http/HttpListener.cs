using System;
using System.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.BodyDecoders;

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
                () => new HttpMessageDecoder(BodyDecoder),
                () => new HttpMessageEncoder());

            
            Configure(config);
        }

        /// <summary>
        /// Used to decode the body of incoming request to form/files.
        /// </summary>
        public IBodyDecoder BodyDecoder { get; set; }

        protected override ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {
            channel.DecoderFailure = OnDecoderFailure;
            //GriffinNetworking.cerchannel.
            return base.OnClientConnected(channel);
        }

        private void OnDecoderFailure(ITcpChannel channel, Exception error)
        {
            var pos = error.Message.IndexOfAny(new[] {'\r', '\n'});
            var descr = pos == -1 ? error.Message : error.Message.Substring(0, pos);
            var response = new HttpResponseBase(HttpStatusCode.BadRequest, descr, "HTTP/1.1");
            channel.Send(response);
            channel.Close();
        }

        protected override void OnMessage(ITcpChannel source, object msg)
        {
            var message = (IHttpMessage) msg;

            // used to be able to send back all
            message.Headers["X-Pipeline-index"] = (_pipelineIndex++).ToString();

            var request = msg as IHttpRequest;
            if (request != null)
                request.RemoteEndPoint = source.RemoteEndpoint;

            base.OnMessage(source, msg);
        }
    }
}
