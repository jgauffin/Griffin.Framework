using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Griffin.Net.Protocols.Http.WebSocket;
using System.IO;

namespace Griffin.Core.Tests.Net.Protocols.Http.WebSocket
{
    public class WebSocketMessageEncoderTests
    {

        [Fact]
        public void binary_message()
        {
            var message = new WebSocketMessage(WebSocketOpcode.Binary, new MemoryStream(new byte[] { 1, 2, 3 }));

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            // rest: payload
            var expected = new byte[] { 130, 3, 1, 2, 3 };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void text_message()
        {
            var message = new WebSocketMessage(WebSocketOpcode.Text, new MemoryStream(Encoding.UTF8.GetBytes("abc")));

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            // rest: payload
            var expected = new byte[] { 129, 3, (byte)'a', (byte)'b', (byte)'c' };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void close_message()
        {
            var message = new WebSocketMessage(WebSocketOpcode.Close);

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            var expected = new byte[] { 136, 0 };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ping_message()
        {
            var message = new WebSocketMessage(WebSocketOpcode.Ping);

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            var expected = new byte[] { 137, 0 };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void pong_message()
        {
            var message = new WebSocketMessage(WebSocketOpcode.Pong);

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            var expected = new byte[] { 138, 0 };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void continuation_message()
        {
            var payload = new byte[WebSocketFrame.FragmentLength + 1];
            payload[WebSocketFrame.FragmentLength] = 1;

            var message = new WebSocketMessage(WebSocketOpcode.Binary, new MemoryStream(payload));

            var buffer = new SocketBufferFake();

            var encoder = new WebSocketEncoder();
            encoder.Prepare(message);
            encoder.Send(buffer);

            // first frame
            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeFalse();

            buffer = new SocketBufferFake();
            encoder.Send(buffer);

            // second frame
            encoder.OnSendCompleted(buffer.BytesTransferred).Should().BeTrue();
            // 1. byte: headers
            // 2. byte: length
            // rest payload
            var expected = new byte[] { 128, 1, 1 };
            buffer.Buffer.Should().BeEquivalentTo(expected);
        }

    }
}
