using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.MicroMsg.Serializers;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    public class MicroMessageDecoderTests
    {
        private int HeaderLengthSize = sizeof (short);

        [Fact]
        public void complete_header_then_half_typename_then_full_message()
        {
            var args = new SocketBufferFake();
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            object actual = null;
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, args.Buffer, 0); //header length
            args.Buffer[2] = MicroMessageDecoder.Version; 
            BitConverter2.GetBytes(body.Length, args.Buffer, 3); //content lkength
            args.Buffer[7] = (byte)(sbyte)type.Length; // type len
            Encoding.ASCII.GetBytes(type, 0, type.Length, args.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength); // type name
            Encoding.ASCII.GetBytes(body, 0, body.Length, args.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength); //body

            var sut = new MicroMessageDecoder(serializer);
            sut.MessageReceived = o => actual = o;
            args.BytesTransferred = sizeof(short) + MicroMessageDecoder.FixedHeaderLength;
            sut.ProcessReadBytes(args);

            args.Offset += args.BytesTransferred;
            args.BytesTransferred = type.Length - 31;
            sut.ProcessReadBytes(args);

            args.Offset += args.BytesTransferred;
            args.BytesTransferred = 31;
            sut.ProcessReadBytes(args);

            args.Offset += args.BytesTransferred;
            args.BytesTransferred = body.Length;
            sut.ProcessReadBytes(args);


            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        [Fact]
        public void partial_header()
        {
            var args = new SocketBufferFake();
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            object actual = null;
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, args.Buffer, 0);
            args.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, args.Buffer, 3);
            args.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, args.Buffer, HeaderLengthSize+MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, args.Buffer, HeaderLengthSize+type.Length + MicroMessageEncoder.FixedHeaderLength);

            var sut = new MicroMessageDecoder(serializer);
            sut.MessageReceived = o => actual = o;
            args.BytesTransferred = MicroMessageDecoder.FixedHeaderLength - 1;
            sut.ProcessReadBytes(args);

            args.Offset += args.BytesTransferred;
            args.BytesTransferred = body.Length + type.Length + 1 + HeaderLengthSize;
            sut.ProcessReadBytes(args);


            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        [Fact]
        public void complete_package()
        {
            var args = new SocketBufferFake();
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            object actual = null;
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, args.Buffer, 0);
            args.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, args.Buffer, 3);
            args.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, args.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, args.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);

            var sut = new MicroMessageDecoder(serializer);
            sut.MessageReceived = o => actual = o;
            args.BytesTransferred = body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength + HeaderLengthSize;
            sut.ProcessReadBytes(args);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }

        [Fact]
        public void half_body()
        {
            var args = new SocketBufferFake();
            var body = "Hello world";
            var type = body.GetType().AssemblyQualifiedName;
            var serializer = new StringSerializer();
            object actual = null;
            BitConverter2.GetBytes(MicroMessageEncoder.FixedHeaderLength + type.Length, args.Buffer, 0);
            args.Buffer[2] = MicroMessageDecoder.Version;
            BitConverter2.GetBytes(body.Length, args.Buffer, 3);
            args.Buffer[7] = (byte)(sbyte)type.Length;
            Encoding.ASCII.GetBytes(type, 0, type.Length, args.Buffer, HeaderLengthSize + MicroMessageEncoder.FixedHeaderLength);
            Encoding.ASCII.GetBytes(body, 0, body.Length, args.Buffer, HeaderLengthSize + type.Length + MicroMessageEncoder.FixedHeaderLength);

            var sut = new MicroMessageDecoder(serializer);
            sut.MessageReceived = o => actual = o;
            args.BytesTransferred = body.Length + type.Length + MicroMessageEncoder.FixedHeaderLength - 5 + HeaderLengthSize;
            sut.ProcessReadBytes(args);
            args.Offset += args.BytesTransferred;
            args.BytesTransferred = 5;
            sut.ProcessReadBytes(args);

            actual.Should().NotBeNull();
            actual.Should().Be("Hello world");
        }


        [Fact]
        public void two_messages()
        {
            var buf = new Byte[] //two msgs with "Hello world" as content
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
            var buffer = new SocketBufferFake();
            buffer.SetBuffer(buf, 0, buf.Length);
            buffer.BytesTransferred = buf.Length;
            var msgs = new List<string>();

            var sut = new MicroMessageDecoder(new DataContractMessageSerializer());
            sut.MessageReceived = o => msgs.Add((string)o);
            sut.ProcessReadBytes(buffer);

            msgs.Count.Should().Be(2);
            msgs[0].Should().StartWith("Hello world");
            msgs[1].Should().StartWith("Hello world");
        }
    }
}