using System;
using System.Threading.Tasks;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Core.Tests.Net.Channels
{
    public class FakeDecoder : IMessageDecoder
    {
        public Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment receiveBuffer)
        {
            return null;
        }
    }
}