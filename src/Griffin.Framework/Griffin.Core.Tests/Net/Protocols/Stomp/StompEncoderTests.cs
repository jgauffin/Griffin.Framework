using System.IO;
using System.Text;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp;
using Griffin.Net.Protocols.Stomp.Frames;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp
{
    public class StompEncoderTests
    {
        [Fact]
        public void message_in_its_simplest_form()
        {
            var frame = new BasicFrame("STOMP");
            var expected = "STOMP\n\n\0";
            var buffer = new SocketBufferFake();

            var encoder = new StompEncoder();
            encoder.Prepare(frame);
            encoder.Send(buffer);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

        [Fact]
        public void send_message()
        {
            var frame = new BasicFrame("SEND");
            frame.AddHeader("destination", "/queue/a");
            frame.AddHeader("receipt", "message-12345");
            frame.Body = new MemoryStream(Encoding.ASCII.GetBytes("hello queue a"));
            var expected = "SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0";
            var buffer = new SocketBufferFake();

            var encoder = new StompEncoder();
            encoder.Prepare(frame);
            encoder.Send(buffer);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

        [Fact]
        public void NoOp_message()
        {
            var frame = new BasicFrame("NoOp");
            var expected = "\n";
            var buffer = new SocketBufferFake();

            var encoder = new StompEncoder();
            encoder.Prepare(frame);
            encoder.Send(buffer);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }
    }
}
