using System;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin.Net.Messaging
{
    public interface IInboundMessagingChannel
    {
        Task<object> ReceiveAsync(CancellationToken token);
        Task<object> ReceiveAsync(TimeSpan timeout);
        Task<object> ReceiveAsync();
    }
}