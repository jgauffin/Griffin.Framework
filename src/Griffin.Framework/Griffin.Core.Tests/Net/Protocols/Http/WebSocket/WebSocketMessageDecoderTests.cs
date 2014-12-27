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

namespace Griffin.Core.Tests.Net.Protocols.Http.WebSocket
{
    public class WebSocketMessageDecoderTests
    {
        /// <summary>
        /// Helper method which sends the websocket upgrade request
        /// </summary>
        /// <param name="decoder"></param>
        private static void sendWebSocketUpgradeRequest(IMessageDecoder decoder)
        {
            var buffer = new SocketBufferFake();
            buffer.Buffer = Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
host: localhost
connection: upgrade
upgrade: websocket

");
            buffer.BytesTransferred = buffer.Buffer.Length;

            decoder.ProcessReadBytes(buffer);
        }

        [Fact]
        public void upgrade()
        {
            IHttpMessage actual = null;

            var decoder = new WebSocketDecoder();
            decoder.MessageReceived = o => actual = (IHttpMessage)o;
            sendWebSocketUpgradeRequest(decoder);

            actual.Headers["host"].Should().Be("localhost");
            actual.Headers["connection"].Should().Be("upgrade");
            actual.Headers["upgrade"].Should().Be("websocket");
        }

        [Fact]
        public void binary_message()
        {
            IWebSocketMessage actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = new byte[] { 130, 3, 1, 2, 3 };
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Binary);
            actual.Payload.Length.Should().Be(3);
            actual.Payload.ReadByte().Should().Be(1);
            actual.Payload.ReadByte().Should().Be(2);
            actual.Payload.ReadByte().Should().Be(3);
        }

        [Fact]
        public void text_message()
        {
            IWebSocketMessage actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = new byte[] { 129, 3, (byte)'a', (byte)'b', (byte)'c' };
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Text);
            actual.Payload.Length.Should().Be(3);
            actual.Payload.ReadByte().Should().Be((int)'a');
            actual.Payload.ReadByte().Should().Be((int)'b');
            actual.Payload.ReadByte().Should().Be((int)'c');
        }

        [Fact]
        public void close_message()
        {
            IWebSocketMessage actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = new byte[] { 136, 0 };
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Close);
            actual.Payload.Length.Should().Be(0);
        }

        [Fact]
        public void ping_message()
        {
            IWebSocketMessage actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = new byte[] { 137, 0 };
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Ping);
            actual.Payload.Length.Should().Be(0);
        }

        [Fact]
        public void pong_message()
        {
            IWebSocketMessage actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = new byte[] { 138, 0 };
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer);

            actual.Opcode.Should().Be(WebSocketOpcode.Pong);
            actual.Payload.Length.Should().Be(0);
        }

        [Fact]
        public void big_message()
        {
            IWebSocketMessage actual = null;

            var buffer1 = new SocketBufferFake();
            buffer1.Buffer = (new byte[] { 2, 126, 255, 255 }).Concat(new byte[WebSocketFrame.FragmentLength]).ToArray(); // continuation frame
            buffer1.BytesTransferred = buffer1.Buffer.Length;

            var buffer2 = new SocketBufferFake();
            buffer2.Buffer = new byte[] { 128, 1, 1 }; // final frame
            buffer2.BytesTransferred = buffer2.Buffer.Length;

            var decoder = new WebSocketDecoder();
            sendWebSocketUpgradeRequest(decoder);
            decoder.MessageReceived = o => actual = (IWebSocketMessage)o;
            decoder.ProcessReadBytes(buffer1);
            decoder.ProcessReadBytes(buffer2);

            actual.Opcode.Should().Be(WebSocketOpcode.Binary);
            actual.Payload.Length.Should().Be(WebSocketFrame.FragmentLength + 1);
            actual.Payload.Position = WebSocketFrame.FragmentLength;
            actual.Payload.ReadByte().Should().Be(1);   
        }

    }
}
