using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Griffin.Net.Protocols.Http.WebSocket;
using Griffin.Core.Tests.Net.Channels;
using Griffin.Net.Protocols.Http;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using NSubstitute;

namespace Griffin.Core.Tests.Net.Protocols.Http.WebSocket
{
    public class WebSocketMessageDecoderTests
    {
        /// <summary>
        /// Helper method which sends the websocket upgrade request
        /// </summary>
        /// <param name="decoder"></param>
        private static async Task<HttpRequest> sendWebSocketUpgradeRequest(IMessageDecoder decoder, IBinaryChannel channel)
        {
            var buf = Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
host: localhost
connection: upgrade
upgrade: websocket

");
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);

            return (HttpRequest)await decoder.DecodeAsync(channel, buffer);
        }

        [Fact]
        public async Task upgrade()
        {
            var decoder = new WebSocketDecoder();
            var channel = Substitute.For<IBinaryChannel>();

            var actual = await sendWebSocketUpgradeRequest(decoder, channel);

            actual.Headers["host"].Should().Be("localhost");
            actual.Headers["connection"].Should().Be("upgrade");
            actual.Headers["upgrade"].Should().Be("websocket");
        }

        [Fact]
        public async Task binary_message()
        {
            var buf = new byte[] { 130, 3, 1, 2, 3 };
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var decoder = new WebSocketDecoder();
            await sendWebSocketUpgradeRequest(decoder, channel);
            var actual = (WebSocketFrame)await decoder.DecodeAsync(channel, buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Binary);
            actual.Payload.Length.Should().Be(3);
            actual.Payload.ReadByte().Should().Be(1);
            actual.Payload.ReadByte().Should().Be(2);
            actual.Payload.ReadByte().Should().Be(3);
        }

        [Fact]
        public async Task text_message()
        {
            var buf = new byte[] { 129, 3, (byte)'a', (byte)'b', (byte)'c' };
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var decoder = new WebSocketDecoder();
            await sendWebSocketUpgradeRequest(decoder, channel);
            var actual = (WebSocketFrame)await decoder.DecodeAsync(channel, buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Text);
            actual.Payload.Length.Should().Be(3);
            actual.Payload.ReadByte().Should().Be((int)'a');
            actual.Payload.ReadByte().Should().Be((int)'b');
            actual.Payload.ReadByte().Should().Be((int)'c');
        }

        [Fact]
        public async Task close_message()
        {
            var buf = new byte[] { 136, 0 };
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var decoder = new WebSocketDecoder();
            await sendWebSocketUpgradeRequest(decoder, channel);
            var actual = (WebSocketFrame)await decoder.DecodeAsync(channel, buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Close);
            actual.Payload.Length.Should().Be(0);
        }

        [Fact]
        public async Task ping_message()
        {
            var buf = new byte[] { 137, 0 };
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var decoder = new WebSocketDecoder();
            await sendWebSocketUpgradeRequest(decoder, channel);
            var actual = (WebSocketFrame)await decoder.DecodeAsync(channel, buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Ping);
            actual.Payload.Length.Should().Be(0);
        }

        [Fact]
        public async Task pong_message()
        {
            var buf = new byte[] { 138, 0 };
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var decoder = new WebSocketDecoder();
            await sendWebSocketUpgradeRequest(decoder, channel);
            var actual = (WebSocketFrame)await decoder.DecodeAsync(channel, buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Pong);
            actual.Payload.Length.Should().Be(0);
        }

        // Correct again in v2.0
        //[Fact]
        //public void big_message()
        //{
        //    IWebSocketMessage actual = null;

        //    var buffer1 = new SocketBufferFake();
        //    buffer1.Buffer = (new byte[] { 2, 126, 255, 255 }).Concat(new byte[WebSocketFrame.FragmentLength]).ToArray(); // continuation frame
        //    buffer1.BytesTransferred = buffer1.Buffer.Length;

        //    var buffer2 = new SocketBufferFake();
        //    buffer2.Buffer = new byte[] { 128, 1, 1 }; // final frame
        //    buffer2.BytesTransferred = buffer2.Buffer.Length;

        //    var decoder = new WebSocketDecoder();
        //    sendWebSocketUpgradeRequest(decoder);
        //    decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
        //    decoder.ProcessReadBytes(buffer1);
        //    decoder.ProcessReadBytes(buffer2);

        //    actual.Opcode.Should().Be(WebSocketOpcode.Binary);
        //    actual.Payload.Length.Should().Be(WebSocketFrame.FragmentLength + 1);
        //    actual.Payload.Position = WebSocketFrame.FragmentLength;
        //    actual.Payload.ReadByte().Should().Be(1);
        //}

    }
}
