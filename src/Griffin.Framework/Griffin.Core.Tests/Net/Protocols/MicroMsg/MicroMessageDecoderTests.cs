using System;
using System.Collections.Generic;
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
    public class MicroMessageDecoderTests
    {
        private int HeaderLengthSize = sizeof(short);

        [Fact]
        public async Task complete_header_then_half_typename_then_full_message()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(65535);
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0); //header length
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3); //content lkength
            buffer.Buffer[7] = (byte)(sbyte)type.Length; // type len
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength); // type name
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength); //body
            var returnSizes = new[]{
                sizeof(short) + MicroMessageDecoder.FixedHeaderLength,
                type.Length - 31,
                31,
                body.Length
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual = await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        private static void GeneratePartialReceives(IBinaryChannel channel, int[] returnSizes)
        {
            int index = 0;
            int offset = 0;
            channel.WhenForAnyArgs(x => x.ReceiveAsync(null))
                .Do(x =>
                {
                    if (index > 0)
                        offset = returnSizes[index - 1];
                    x.Arg<IBufferSegment>().Offset = offset;
                    x.Arg<IBufferSegment>().Count = returnSizes[index];
                });
            channel.ReceiveAsync(Arg.Any<IBufferSegment>()).Returns(returnSizes[index++]);
        }

        [Fact]
        public async Task partial_header()
        {
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0);
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3);
            buffer.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);
            var returnSizes = new[]{
                MicroMessageDecoder.FixedHeaderLength - 1,
                body.Length + type.Length + 1 + HeaderLengthSize
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual=await sut.DecodeAsync(channel, buffer);


            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        [Fact]
        public async Task complete_package()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(65535);
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0);
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3);
            buffer.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);
            var returnSizes = new[]{
                body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength + HeaderLengthSize            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual=await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        [Fact]
        public async Task decoder_can_be_reused()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(65535);
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0);
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3);
            buffer.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);
            var returnSizes = new[]{
                body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength + HeaderLengthSize,
                body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength + HeaderLengthSize,
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual = await sut.DecodeAsync(channel, buffer);
            sut.Clear();
            var actual2 = await sut.DecodeAsync(channel, buffer);

            actual2.Should().NotBeNull();
            actual2.Should().Be("Hello world");
        }

        [Fact]
        public async Task can_clear_even_if_no_messages_have_been_received()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(65535);
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0);
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3);
            buffer.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);
            var returnSizes = new[]{
                body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength + HeaderLengthSize
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual = await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }


        [Fact]
        public async Task half_body()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(65535);
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, buffer.Buffer, 0);
            buffer.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, buffer.Buffer, 3);
            buffer.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, buffer.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, buffer.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);
            var returnSizes = new[]{
                body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength - 5 + HeaderLengthSize
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var actual = await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }


        [Fact]
        public async Task two_messages()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf = new byte[] //two msgs with "Hello world" as content
            {
                96, 0, 1, 88, 0, 0, 0, 90, 83, 121, 115, 116, 101, 109, 46, 83, 116, 114, 105, 110, 103, 44, 32, 109, 115,
                99, 111, 114, 108, 105, 98, 44, 32, 86, 101, 114, 115, 105, 111, 110, 61, 52, 46, 48, 46, 48, 46, 48, 44,
                32, 67, 117, 108, 116, 117, 114, 101, 61, 110, 101, 117, 116, 114, 97, 108, 44, 32, 80, 117, 98, 108,
                105, 99, 75, 101, 121, 84, 111, 107, 101, 110, 61, 98, 55, 55, 97, 53, 99, 53, 54, 49, 57, 51, 52, 101,
                48, 56, 57, 60, 115, 116, 114, 105, 110, 103, 32, 120, 109, 108, 110, 115, 61, 34, 104, 116, 116, 112,
                58, 47, 47, 115, 99, 104, 101, 109, 97, 115, 46, 109, 105, 99, 114, 111, 115, 111, 102, 116, 46, 99, 111,
                109, 47, 50, 48, 48, 51, 47, 49, 48, 47, 83, 101, 114, 105, 97, 108, 105, 122, 97, 116, 105, 111, 110,
                47, 34, 62, 72, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 60, 47, 115, 116, 114, 105, 110, 103,
                62,
                96, 0, 1, 88, 0, 0, 0, 90, 83, 121, 115, 116, 101, 109, 46, 83, 116, 114, 105, 110, 103, 44, 32, 109, 115,
                99, 111, 114, 108, 105, 98, 44, 32, 86, 101, 114, 115, 105, 111, 110, 61, 52, 46, 48, 46, 48, 46, 48, 44,
                32, 67, 117, 108, 116, 117, 114, 101, 61, 110, 101, 117, 116, 114, 97, 108, 44, 32, 80, 117, 98, 108,
                105, 99, 75, 101, 121, 84, 111, 107, 101, 110, 61, 98, 55, 55, 97, 53, 99, 53, 54, 49, 57, 51, 52, 101,
                48, 56, 57, 60, 115, 116, 114, 105, 110, 103, 32, 120, 109, 108, 110, 115, 61, 34, 104, 116, 116, 112,
                58, 47, 47, 115, 99, 104, 101, 109, 97, 115, 46, 109, 105, 99, 114, 111, 115, 111, 102, 116, 46, 99, 111,
                109, 47, 50, 48, 48, 51, 47, 49, 48, 47, 83, 101, 114, 105, 97, 108, 105, 122, 97, 116, 105, 111, 110,
                47, 34, 62, 72, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 60, 47, 115, 116, 114, 105, 110, 103,
                62
            };
            var serializer = new StringSerializer();
            var buffer = new StandAloneBuffer(buf, 0, buf.Length);
            var returnSizes = new[]{
                buf.Length
            };
            GeneratePartialReceives(channel, returnSizes);

            var sut = new MicroMessageDecoder(serializer);
            var msg1 = (string)await sut.DecodeAsync(channel, buffer);
            var msg2 = (string)await sut.DecodeAsync(channel, buffer);

            msg1.Should().StartWith("Hello world");
            msg2.Should().StartWith("Hello world");
        }
    }
}