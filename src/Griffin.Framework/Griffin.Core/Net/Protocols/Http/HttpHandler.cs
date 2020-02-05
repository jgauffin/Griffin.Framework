using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.Results;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     One handler is generated per connection
    /// </summary>
    public class HttpHandler : IClientHandler<HttpContext>
    {
        private static BufferManager BufMgr;
        private readonly IBinaryChannel _channel;
        private readonly HttpContext _httpContext;
        private readonly HttpMessageDecoder _messageDecoder;
        private readonly IMessageEncoder _messageEncoder;
        private readonly MessagingServerPipeline<HttpContext> _pipeline;
        private readonly IBufferSegment _receiveBuffer;
        private bool _running = true;

        public HttpHandler(Socket socket,
            IBufferSegment receiveBuffer,
            MessagingServerPipeline<HttpContext> pipeline,
            IMessageSerializer messageSerializer = null)
        {
            _channel = new TcpChannel();
            _channel.Assign(socket);
            _messageDecoder = new HttpMessageDecoder(messageSerializer ?? new CompositeMessageSerializer());
            _messageEncoder = new HttpMessageEncoder();
            _receiveBuffer = receiveBuffer;
            _pipeline = pipeline;
            _httpContext = new HttpContext(_channel.ChannelData);
        }

        public HttpHandler(
            IBinaryChannel channel,
            IBufferSegment receiveBuffer,
            MessagingServerPipeline<HttpContext> pipeline,
            IMessageSerializer messageSerializer = null)
        {
            _channel = channel;
            _messageDecoder = new HttpMessageDecoder(messageSerializer ?? new CompositeMessageSerializer());
            _messageEncoder = new HttpMessageEncoder();
            _receiveBuffer = receiveBuffer;
            _pipeline = pipeline;
            _httpContext = new HttpContext(_channel.ChannelData);
        }

        public string ServerName { get; set; }

        public IChannelData ChannelData => _channel.ChannelData;

        public virtual async Task ProcessAsync(MessagingServerPipeline<HttpContext> pipline)
        {
            while (_running)
            {
                try
                {
                    var msg = await _messageDecoder.DecodeAsync(_channel, _receiveBuffer);
                    _httpContext.Request = (HttpRequest) msg;
                    _httpContext.Response = _httpContext.Request.CreateResponse();

                    if (!string.IsNullOrEmpty(ServerName))
                        _httpContext.Response.Headers["Server"] = ServerName;
                    _httpContext.Response.Headers["Date"] = DateTime.UtcNow.ToString("R");

                    await ExecutePipelineAsync(_httpContext);
                    await _messageEncoder.EncodeAsync(_httpContext.Response, _channel);
                }
                catch (Exception ex)
                {
                    if (!_running)
                        break;

                    var result = new ServerErrorResult(ex);
                    _pipeline.ProcessError(_httpContext, result);
                    if (result.Shutdown)
                        _running = false;
                }
            }
        }

        public async Task CloseAsync()
        {
            _running = false;
            await _channel.CloseAsync();
        }

        public void Reset()
        {
            _messageEncoder.Clear();
        }

        public static HttpHandler CreateSecure(MessagingServerHandlerFactoryContext context,
            X509Certificate certificate, MessagingServerPipeline<HttpContext> pipeline,
            IBufferSegment receiveBuffer = null)
        {
            if (receiveBuffer == null)
            {
                if (BufMgr == null)
                    BufMgr = new BufferManager(10);

                receiveBuffer = BufMgr.Dequeue();
            }

            var builder = new ServerSideSslStreamBuilder(certificate);
            var channel = new SecureTcpChannel(builder);
            channel.Assign(context.Socket);
            return new HttpHandler(channel, receiveBuffer, pipeline);
        }

        public async Task Encode(object message)
        {
            await _messageEncoder.EncodeAsync(message, _channel);
        }

        protected virtual async Task ExecutePipelineAsync(HttpContext context)
        {
            try
            {
                await _pipeline.Execute(_httpContext);
            }
            catch (Exception ex)
            {
                var result = new ServerErrorResult(ex);
                _pipeline.ProcessResult(_httpContext, result);
            }
        }
    }
}