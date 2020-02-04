using Griffin.Net.Channels;

namespace Griffin.Net
{
    public interface IMiddlewareContext
    {
        IChannelData ChannelData { get; }
    }
}