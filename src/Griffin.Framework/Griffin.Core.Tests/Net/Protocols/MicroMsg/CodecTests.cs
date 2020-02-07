using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
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
            var encoder = new MicroMessageEncoder(serializer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = "Hello world";
            var channel = new FakeChannel();

            await encoder.EncodeAsync(expected, channel);
            channel.ResetPosition();
            var actual = await decoder.DecodeAsync(channel, buffer);

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task codec_a_stream()
        {
            var buffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = "Hello world";
            var buf = Encoding.ASCII.GetBytes(expected);
            var stream = new MemoryStream(buf, 0, buf.Length, true, true);
            stream.SetLength(expected.Length);
            var channel = new FakeChannel();

            await encoder.EncodeAsync(stream, channel);
            channel.ResetPosition();
            var actual = await decoder.DecodeAsync(channel, buffer);

            var reader = new StreamReader((Stream) actual);
            var msg = reader.ReadToEnd();
            msg.Should().Be(expected);
        }

        [Fact]
        public async Task codec_a_byte_buffer()
        {
            var receiveBuffer = new StandAloneBuffer(65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = new byte[] {1, 2, 3, 4, 5, 6, 7};
            var channel = new FakeChannel();

            await encoder.EncodeAsync(expected, channel);
            channel.ResetPosition();
            var actual = await decoder.DecodeAsync(channel, receiveBuffer);

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
            var encoder = new MicroMessageEncoder(serializer);
            var decoder = new MicroMessageDecoder(serializer);
            var expected = new CustomType {Name = "Arne"};
            var channel = new FakeChannel();

            await encoder.EncodeAsync(expected, channel);
            channel.ResetPosition();
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
