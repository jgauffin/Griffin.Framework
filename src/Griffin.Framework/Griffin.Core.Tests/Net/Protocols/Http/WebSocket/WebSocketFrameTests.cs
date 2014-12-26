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
    public class WebSocketFrameTests
    {

        [Fact]
        public void empty_frame()
        {
            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Text, WebSocketMask.Unmask, null);

            frame.Fin.Should().Be(WebSocketFin.Final);
            frame.Opcode.Should().Be(WebSocketOpcode.Text);
            frame.Mask.Should().Be(WebSocketMask.Unmask);
            frame.Rsv1.Should().Be(WebSocketRsv.Off);
            frame.Rsv2.Should().Be(WebSocketRsv.Off);
            frame.Rsv3.Should().Be(WebSocketRsv.Off);
            frame.MaskingKey.Should().BeNullOrEmpty();
            frame.PayloadLength.Should().Be(0);
            frame.ExtPayloadLength.Should().BeNullOrEmpty();
        }

        [Fact]
        public void small_start_frame()
        {
            var buffer = new byte[1];

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Binary, WebSocketMask.Unmask, new MemoryStream(buffer));

            frame.PayloadLength.Should().Be(1);
            frame.ExtPayloadLength.Should().BeNullOrEmpty();
        }

        [Fact]
        public void small_end_frame()
        {
            var buffer = new byte[125];

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Binary, WebSocketMask.Unmask, new MemoryStream(buffer));

            frame.PayloadLength.Should().Be(125);
            frame.ExtPayloadLength.Should().BeNullOrEmpty();
        }

        [Fact]
        public void medium_start_frame()
        {
            var buffer = new byte[126];

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Binary, WebSocketMask.Unmask, new MemoryStream(buffer));

            frame.PayloadLength.Should().Be(126);
            frame.ExtPayloadLength.Should().BeEquivalentTo(new byte[] { 0, 126 });
        }

        [Fact]
        public void medium_end_frame()
        {
            var buffer = new byte[0x00FFFF];

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Binary, WebSocketMask.Unmask, new MemoryStream(buffer));

            frame.PayloadLength.Should().Be(126);
            frame.ExtPayloadLength.Should().BeEquivalentTo(new byte[] { 255, 255 });
        }

        [Fact]
        public void big_start_frame()
        {
            var buffer = new byte[0x010000];

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Binary, WebSocketMask.Unmask, new MemoryStream(buffer));

            frame.PayloadLength.Should().Be(127);
            frame.ExtPayloadLength.Should().BeEquivalentTo(new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 });
        }

        [Fact]
        public void masked_frame()
        {
            var buffer = new byte[8];
            var maskingKey = new byte[4];

            new Random().NextBytes(buffer);
            new Random().NextBytes(maskingKey);

            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketRsv.Off, WebSocketRsv.Off, WebSocketRsv.Off, WebSocketOpcode.Binary, WebSocketMask.Mask, maskingKey, (byte)4, new byte[0], new MemoryStream(buffer));

            frame.Mask.Should().Be(WebSocketMask.Mask);
            frame.MaskingKey.Should().Equal(maskingKey);
            frame.PayloadLength.Should().Be(4);

            frame.Unmask();

            frame.Mask.Should().Be(WebSocketMask.Unmask);
            frame.MaskingKey.Should().BeNullOrEmpty();
            frame.Payload.Position.Should().Be(0);

            frame.Payload.ReadByte().Should().Be(buffer[0] ^ maskingKey[0]);
            frame.Payload.ReadByte().Should().Be(buffer[1] ^ maskingKey[1]);
            frame.Payload.ReadByte().Should().Be(buffer[2] ^ maskingKey[2]);
            frame.Payload.ReadByte().Should().Be(buffer[3] ^ maskingKey[3]);
            frame.Payload.ReadByte().Should().Be(buffer[4] ^ maskingKey[0]);
            frame.Payload.ReadByte().Should().Be(buffer[5] ^ maskingKey[1]);
            frame.Payload.ReadByte().Should().Be(buffer[6] ^ maskingKey[2]);
            frame.Payload.ReadByte().Should().Be(buffer[7] ^ maskingKey[3]);

            Assert.DoesNotThrow(() => frame.Unmask());
        }

    }
}
