using System;
using System.Security.Claims;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http
{
    public class HttpContext : IMiddlewareContext
    {
        public HttpContext(IChannelData channelData)
        {
            ChannelData = channelData ?? throw new ArgumentNullException(nameof(channelData));
        }

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpResult Result { get; set; }

        public ClaimsPrincipal User { get; set; }


        public IChannelData ChannelData { get; }
    }
}