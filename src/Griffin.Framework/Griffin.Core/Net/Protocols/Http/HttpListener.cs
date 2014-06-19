using System;
using System.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// HTTP listener
    /// </summary>
    /// <remarks>
    /// <para>
    /// Will produce <see cref="HttpRequestBase"/> unless you change the <see cref="BodyDecoder"/> property, which will make the listener produce <see cref="HttpRequest"/> instead.
    /// </para>
    /// </remarks>
    public class HttpListener : ChannelTcpListener
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public HttpListener(ChannelTcpListenerConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        public HttpListener()
        {
            var config = new ChannelTcpListenerConfiguration(
                () => new HttpMessageDecoder(BodyDecoder),
                () => new HttpMessageEncoder());

            Configure(config);
        }

        /// <summary>
        /// Used to decode the body of incoming request to form/files.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Per default <c>null</c> which means that nothing will be done with the body by the library.
        /// </para>
        /// </remarks>
        public IMessageSerializer BodyDecoder { get; set; }

        protected override ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {
            channel.ChannelFailure = OnDecoderFailure;
            //GriffinNetworking.cerchannel.
            return base.OnClientConnected(channel);
        }

        /// <summary>
        ///     Start this listener
        /// </summary>
        /// <param name="address">Address to accept connections on</param>
        /// <param name="port">Port to use. Set to <c>0</c> to let the OS decide which port to use. </param>
        /// <seealso cref="ChannelTcpListener.LocalPort" />
        public override void Start(IPAddress address, int port)
        {
            if (ChannelFactory.OutboundMessageQueueFactory == null)
                ChannelFactory.OutboundMessageQueueFactory = () => new PipelinedMessageQueue();

            base.Start(address, port);
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

            //TODO: Try again if we fail to update
            var counter = (int)source.Data.GetOrAdd(HttpMessage.PipelineIndexKey, x => 0) + 1;
            source.Data.TryUpdate(HttpMessage.PipelineIndexKey, counter, counter - 1);

            message.Headers[HttpMessage.PipelineIndexKey] = counter.ToString();

            var request = msg as IHttpRequest;
            if (request != null)
                request.RemoteEndPoint = source.RemoteEndpoint;

            base.OnMessage(source, msg);
        }
    }
}
