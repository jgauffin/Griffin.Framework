using System.Threading.Tasks;

namespace Griffin.Net.Messaging
{
    public interface IOutboundMessagingChannel
    {
        Task SendAsync(object message);
    }
}