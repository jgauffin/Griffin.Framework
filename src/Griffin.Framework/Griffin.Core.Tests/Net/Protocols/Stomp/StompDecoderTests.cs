using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp;
using Griffin.Net.Protocols.Stomp.Frames;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp
{
    public class StompDecoderTests
    {
        [Fact]
        public void decode_a_send_message()
        {
            BasicFrame actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual = (BasicFrame)o;
            decoder.ProcessReadBytes(buffer);

            actual.Name.Should().Be("SEND");
            actual.Headers.Count.Should().Be(3);
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["destination"].Should().Be("/queue/a");
            actual.Headers["receipt"].Should().Be("message-12345");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void noop_lines_should_be_ignored()
        {
            BasicFrame actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "\n\n\n\n\n\nSEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual = (BasicFrame)o;
            decoder.ProcessReadBytes(buffer);

            actual.Name.Should().Be("SEND");
            actual.Headers.Count.Should().Be(3);
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["destination"].Should().Be("/queue/a");
            actual.Headers["receipt"].Should().Be("message-12345");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }


        [Fact]
        public void decode_two_messages()
        {
            var actual = new List<BasicFrame>();
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual.Add((BasicFrame)o);
            decoder.ProcessReadBytes(buffer);

            actual[1].Name.Should().Be("SEND");
            actual[1].Headers.Count.Should().Be(3);
            actual[1].Body.Should().NotBeNull();
            actual[1].Body.Length.Should().Be(13);
            actual[1].Headers["destination"].Should().Be("/queue/a");
            actual[1].Headers["receipt"].Should().Be("message-12345");
            actual[1].Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual[1].Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void decode_two_halves_where_the_body_is_partial()
        {
            BasicFrame actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual = (BasicFrame)o;

            buffer.BytesTransferred = buffer.Buffer.Length - 10;
            decoder.ProcessReadBytes(buffer);
            buffer.Offset = buffer.BytesTransferred;
            buffer.BytesTransferred = 10;
            decoder.ProcessReadBytes(buffer);

            actual.Name.Should().Be("SEND");
            actual.Headers.Count.Should().Be(3);
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["destination"].Should().Be("/queue/a");
            actual.Headers["receipt"].Should().Be("message-12345");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void decode_two_halves_where_the_header_is_partial()
        {
            BasicFrame actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual = (BasicFrame)o;

            buffer.BytesTransferred = 10;
            decoder.ProcessReadBytes(buffer);
            buffer.Offset = 10;
            buffer.BytesTransferred = buffer.Buffer.Length - 10;
            decoder.ProcessReadBytes(buffer);

            actual.Name.Should().Be("SEND");
            actual.Headers.Count.Should().Be(3);
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["destination"].Should().Be("/queue/a");
            actual.Headers["receipt"].Should().Be("message-12345");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void header_only_message_sprinkled_with_a_litte_bit_of_NoOp_lines()
        {
            BasicFrame actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "\n\n\n\n\n\nSEND\ndestination:/queue/a\nreceipt:message-12345\n\n\0");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual = (BasicFrame)o;
            decoder.ProcessReadBytes(buffer);

            actual.Name.Should().Be("SEND");
            actual.Headers.Count.Should().Be(2);
            actual.Body.Should().BeNull();
            actual.Headers["destination"].Should().Be("/queue/a");
            actual.Headers["receipt"].Should().Be("message-12345");
        }

        [Fact]
        public void header_only_message_sprinkled_with_a_litte_bit_of_NoOp_lines__and_finally_a_regular_message()
        {
            var actual = new List<BasicFrame>();
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    "\n\n\n\n\n\nSEND\ndestination:/queue/a\nreceipt:message-12345\n\n\0SEND\ndestination:/queue/a\nreceipt:message-12345\ncontent-length:13\n\nhello queue a\0");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new StompDecoder();
            decoder.MessageReceived = o => actual.Add((BasicFrame)o);
            decoder.ProcessReadBytes(buffer);

            actual[0].Name.Should().Be("SEND");
            actual[0].Headers.Count.Should().Be(2);
            actual[0].Body.Should().BeNull();
            actual[0].Headers["destination"].Should().Be("/queue/a");
            actual[0].Headers["receipt"].Should().Be("message-12345");
            actual[1].Name.Should().Be("SEND");
            actual[1].Headers.Count.Should().Be(3);
            actual[1].Body.Should().NotBeNull();
            actual[1].Body.Length.Should().Be(13);
            actual[1].Headers["destination"].Should().Be("/queue/a");
            actual[1].Headers["receipt"].Should().Be("message-12345");
            actual[1].Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual[1].Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

    }
}