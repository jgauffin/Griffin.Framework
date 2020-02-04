using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.Tests.Net.Buffers;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Strings;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Strings
{
    public class StringEncoderTests
    {
        [Fact]
        public async Task send_simple_message()
        {
            var expected = "Hello world!";
            var buf = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var sut = new StringEncoder();
            await sut.EncodeAsync(expected, channel);

            BitConverter.ToInt32(buf.Buffer, 0).Should().Be(expected.Length);
            var actual = Encoding.UTF8.GetString(buf.Buffer, buf.StartOffset + 4, buf.Count - 4);
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task send_partial_message()
        {
            var expected = "Hello world!";
            var buf = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var sut = new StringEncoder();
            buf.Count = 5;
            await sut.EncodeAsync(expected, channel);
            buf.Offset = 5;
            buf.Count = expected.Length - 5;
            await sut.EncodeAsync(expected, channel);

            BitConverter.ToInt32(buf.Buffer, 0).Should().Be(expected.Length);
            var actual = Encoding.UTF8.GetString(buf.Buffer, buf.Offset, buf.Count);
            actual.Should().Be("ello world!");
        }

       
    }
}
