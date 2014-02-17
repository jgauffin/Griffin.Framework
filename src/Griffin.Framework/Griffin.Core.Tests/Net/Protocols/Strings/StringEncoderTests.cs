using System;
using System.Text;
using FluentAssertions;
using Griffin.Net.Protocols.Strings;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Strings
{
    public class StringEncoderTests
    {
        [Fact]
        public void send_simple_message()
        {
            var buf = new SocketBufferFake();
            var expected = "Hello world!";

            var sut = new StringEncoder();
            sut.Prepare(expected);
            sut.Send(buf);

            BitConverter.ToInt32(buf.Buffer, 0).Should().Be(expected.Length);
            var actual = Encoding.UTF8.GetString(buf.Buffer, buf.BaseOffset + 4, buf.Count - 4);
            actual.Should().Be(expected);
        }

        [Fact]
        public void send_partial_message()
        {
            var buf = new SocketBufferFake();
            var expected = "Hello world!";

            var sut = new StringEncoder();
            sut.Prepare(expected);
            sut.Send(buf);
            sut.OnSendCompleted(5);
            sut.Send(buf);

            BitConverter.ToInt32(buf.Buffer, 0).Should().Be(expected.Length);
            var actual = Encoding.UTF8.GetString(buf.Buffer, buf.Offset, buf.Count);
            actual.Should().Be("ello world!");
        }

       
    }
}
