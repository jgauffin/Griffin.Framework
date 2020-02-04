using System;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public abstract class WebSocketMiddleware : IMiddleware<WebsocketContext>
    {
        public abstract Task Process(WebsocketContext context, Func<Task> next);
    }
}