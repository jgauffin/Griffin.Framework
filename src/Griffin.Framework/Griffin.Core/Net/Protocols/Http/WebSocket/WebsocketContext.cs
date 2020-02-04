using System;
using System.Threading.Tasks;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebsocketContext : IMiddlewareContext
    {
        private readonly Func<WebSocketMessage, Task> _sendAsync;

        public WebsocketContext(IChannelData channelData, WebSocketRequest request, Func<WebSocketMessage, Task> sendAsync)
        {
            _sendAsync = sendAsync;
            ChannelData = channelData;
            Request = request;
        }

        public IChannelData ChannelData { get; private set; }

        public WebSocketRequest Request { get; private set; }

        public async Task Reply(WebSocketMessage message)
        {
            await _sendAsync(message);
        }

    }
}