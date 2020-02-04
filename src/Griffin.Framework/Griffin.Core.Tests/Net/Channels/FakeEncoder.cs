using System;
using System.Threading.Tasks;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Core.Tests.Net.Channels
{
    public class FakeEncoder : IMessageEncoder
    {
        public Task EncodeAsync(object message, IBinaryChannel channel)
        {
            Message = message;
            return Task.CompletedTask;
        }

        public object Message { get; set; }

        public void Clear()
        {
        }
    }
}