using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.Results;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    ///     A HttpListener that automatically transitions all incoming requests to WebSocket protocol.
    /// </summary>
    public class WebSocketHandler : HttpHandler
    {
        private readonly IBinaryChannel _channel;
        private readonly IBufferSegment _receiveBuffer;
        private readonly WebSocketDecoder _decoder = new WebSocketDecoder();
        private readonly WebSocketEncoder _encoder = new WebSocketEncoder();
        private bool _isUpgraded;
        private MessagingServerPipeline<WebsocketContext> _pipeline;

        public WebSocketHandler(IBinaryChannel channel, IBufferSegment receiveBuffer,
            MessagingServerPipeline<HttpContext> httpPipeline,
            MessagingServerPipeline<WebsocketContext> websocketPipeline) : base(channel, receiveBuffer, httpPipeline)
        {
            _channel = channel;
            _receiveBuffer = receiveBuffer;
            _pipeline = websocketPipeline;
        }

        public override async Task ProcessAsync(MessagingServerPipeline<HttpContext> pipline)
        {
            if (!_isUpgraded)
            {
                await base.ProcessAsync(pipline);
                return;
            }


            var webSocketMessage = (WebSocketMessage) await _decoder.DecodeAsync(_channel, _receiveBuffer);

            // standard message responses handled by listener
            switch (webSocketMessage.OpCode)
            {
                case WebSocketOpcode.Ping:
                    await SendAsync(new WebSocketMessage(WebSocketOpcode.Pong, webSocketMessage.Payload));
                    return;
                case WebSocketOpcode.Close:
                    await SendAsync(new WebSocketMessage(WebSocketOpcode.Close));
                    Close();
                    return;
            }

            var context = new WebsocketContext(_channel.ChannelData, (WebSocketRequest) webSocketMessage, SendAsync);
            await _pipeline.Execute(context);
        }

        protected override Task ExecutePipelineAsync(HttpContext context)
        {
            var httpMessage = context.Request as HttpMessage;
            if (WebSocketUtils.IsWebSocketUpgrade(httpMessage))
            {
                ProcessUpgrade(context, httpMessage);
                return Task.FromResult<object>(null);
            }

            return base.ExecutePipelineAsync(context);
        }

        private void Close()
        {
        }

        private static void ProcessUpgrade(HttpContext context, HttpMessage httpMessage)
        {
            if (httpMessage is HttpRequest) // server mode
            {
                var webSocketKey = httpMessage.Headers["Sec-WebSocket-Key"];

                // TODO: why not provide the response in the WebSocketClientConnectEventArgs event?
                var response = WebSocketResponses.Upgrade(webSocketKey);
                context.Result = new HttpResponseResult(response);
            }
        }

        private async Task SendAsync(WebSocketMessage message)
        {
            await _encoder.EncodeAsync(message, _channel);
        }
    }
}