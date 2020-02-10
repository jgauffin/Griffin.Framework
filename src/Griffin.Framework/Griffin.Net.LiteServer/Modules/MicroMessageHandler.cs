using System;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.LiteServer.Modules.Authentication;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.LiteServer.Modules
{
    public class MicroMessageHandler : IClientHandler<MicroMessageContext>
    {
        private readonly IBinaryChannel _channel;
        private readonly MicroMessageContext _context;
        private readonly MicroMessageDecoder _decoder;
        private readonly MicroMessageEncoder _encoder;
        private IBufferSegment _receiveBuffer;

        public MicroMessageHandler(IMessageSerializer messageSerializer, IBinaryChannel channel)
        {
            _channel = channel;
            if (!channel.IsConnected)
                throw new ChannelException("Channel is not connected.");

            _encoder = new MicroMessageEncoder(messageSerializer);
            _decoder = new MicroMessageDecoder(messageSerializer);
            _context = new MicroMessageContext();
            _receiveBuffer = new StandAloneBuffer(65535);
            ChannelData = _context.ChannelData;
        }

        public IChannelData ChannelData { get; }

        public async Task ProcessAsync(MessagingServerPipeline<MicroMessageContext> context)
        {
            while (true)
            {
                var msg = await _decoder.DecodeAsync(_channel, _receiveBuffer);
                _context.RequestMessage = msg;
                await context.Execute(_context);

                if (_context.Response != null)
                    await _encoder.EncodeAsync(_context.Response, _channel);
            }
        }

        public async Task CloseAsync()
        {
            await _channel.CloseAsync();
        }

        public void Reset()
        {
            _decoder.Clear();
        }
    }
}