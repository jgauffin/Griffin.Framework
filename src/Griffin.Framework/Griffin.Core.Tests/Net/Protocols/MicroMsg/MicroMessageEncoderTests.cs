using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols.MicroMsg;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    
    public  class MicroMessageEncoderTests
    {
        [Fact]
        public void dont_close_our_internal_stream()
        {
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var msg = "Hello world";
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.Clear();

            var field = sut.GetType().GetField("_bodyStream", BindingFlags.Instance | BindingFlags.NonPublic);
            ((Stream) field.GetValue(sut)).CanWrite.Should().BeTrue();
        }

        [Fact]
        public void close_external_Stream()
        {
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var msg = new MemoryStream();
            var text = Encoding.ASCII.GetBytes("Hello world");
            msg.Write(text, 0, text.Length);
            msg.Position = 0;
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.Clear();

            var field = sut.GetType().GetField("_bodyStream", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new ArgumentNullException("field");
            var value = ((Stream)field.GetValue(sut));
            value.Should().BeNull();
        }


        [Fact]
        public void write_a_complete_string_message_directly()
        {
            var contentType = "text/plain;type=System.String";
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var msg = "Hello world";
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);

            var headerLen = MicroMessageEncoder.FixedHeaderLength + contentType.Length;
            BitConverter.ToInt16(buffer.Buffer, 0).Should().Be((short)headerLen);
            buffer.Buffer[2].Should().Be(1, "first version");
            BitConverter.ToInt32(buffer.Buffer, 3).Should().Be(msg.Length);
            buffer.Buffer[7].Should().Be((byte)contentType.Length);
            Encoding.ASCII.GetString(buffer.Buffer, 8, contentType.Length).Should().Be(contentType);
            Encoding.ASCII.GetString(buffer.Buffer, 2 + MicroMessageEncoder.FixedHeaderLength + contentType.Length, msg.Length)
                .Should()
                .Be(msg);
        }

        [Fact]
        public void partial_send__continue_sending_rest_of_the_buffer_before_doing_anything_else()
        {
            var contentType = "text/plain;type=System.String";
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var msg = "Hello world";
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.OnSendCompleted(10);
            sut.Send(buffer);

            buffer.Offset.Should().Be(10);
            buffer.Count.Should().Be(2+ MicroMessageEncoder.FixedHeaderLength + contentType.Length + msg.Length - 10);
        }

        [Fact]
        public void too_small_buffer_requires_multiple_sends()
        {
            var contentType = "text/plain;type=System.String";
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[520], 0, 520);
            var msg = "Hello world".PadRight(520);
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.OnSendCompleted(520).Should().BeFalse();
            sut.Send(buffer);

            buffer.Offset.Should().Be(0);
            // headerlength + fixed header length + content type length + content type - sent in first batch
            buffer.Count.Should().Be(2 + MicroMessageEncoder.FixedHeaderLength + contentType.Length + msg.Length - 520);
        }

        [Fact]
        public void reset_after_each_successful_message()
        {
            var contentType = "text/plain;type=System.String";
            var serializer = new StringSerializer();
            var slice = new BufferSlice(new byte[520], 0, 520);
            var msg = "Hello world";
            var buffer = new SocketBufferFake();

            var sut = new MicroMessageEncoder(serializer, slice);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.OnSendCompleted(13);
            sut.Send(buffer);
            sut.OnSendCompleted(2 + MicroMessageEncoder.FixedHeaderLength + contentType.Length + msg.Length - 13);
            sut.Prepare(msg);
            sut.Send(buffer);
            sut.OnSendCompleted(5);
            sut.Send(buffer);
            

            buffer.Offset.Should().Be(5);
            buffer.Count.Should().Be(2 + MicroMessageEncoder.FixedHeaderLength + contentType.Length + msg.Length - 5);
        }
    }
}
