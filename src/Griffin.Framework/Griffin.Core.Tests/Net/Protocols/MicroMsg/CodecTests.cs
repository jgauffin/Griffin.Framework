using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    /// <summary>
    /// Integration tests between the encoder and decoder.
    /// </summary>
    public class CodecTests
    {
        [Fact]
        public async Task codec_a_string_message()
        {
            var buffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, buffer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = "Hello world";
            var channel = Substitute.For<IBinaryChannel>();

            await encoder.EncodeAsync(expected, channel);
            buffer.Offset = 0;
            var actual = await decoder.DecodeAsync(channel, buffer);

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task codec_a_stream()
        {
            var buffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, buffer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = "Hello world";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(expected));
            stream.SetLength(expected.Length);
            var channel = Substitute.For<IBinaryChannel>();

            await encoder.EncodeAsync(expected, channel);
            buffer.Offset = 0;
            var actual = await decoder.DecodeAsync(channel, buffer);

            var reader = new StreamReader((Stream) actual);
            var msg = reader.ReadToEnd();
            msg.Should().Be(expected);
        }

        [Fact]
        public async Task codec_a_byte_buffer()
        {
            var buffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, buffer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = new byte[] {1, 2, 3, 4, 5, 6, 7};
            var channel = Substitute.For<IBinaryChannel>();

            await encoder.EncodeAsync(expected, channel);
            buffer.Offset = 0;
            var actual = await decoder.DecodeAsync(channel, buffer);

            var ms = (MemoryStream) actual;
            var buf = ms.GetBuffer();
            expected.Should().BeSubsetOf(buf);
            ms.Length.Should().Be(expected.Length);
        }

        [Fact]
        public async Task codec_a_custom_type()
        {
            var buffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, buffer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = new CustomType {Name = "Arne"};
            var channel = Substitute.For<IBinaryChannel>();

            await encoder.EncodeAsync(expected, channel);
            buffer.Offset = 0;
            var actual = await decoder.DecodeAsync(channel, buffer);

            actual.Should().BeOfType<CustomType>();
            actual.As<CustomType>().Name.Should().Be(expected.Name);
        }


        public class CustomType
        {
            public string Name { get; set; }
        }
    }
}
