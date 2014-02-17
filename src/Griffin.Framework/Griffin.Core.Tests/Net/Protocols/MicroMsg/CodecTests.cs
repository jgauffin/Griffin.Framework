using System.IO;
using System.Text;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.MicroMsg.Serializers;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    /// <summary>
    /// Integration tests between the encoder and decoder.
    /// </summary>
    public class CodecTests
    {
        [Fact]
        public void codec_a_string_message()
        {
            object receivedMessage = null;
            var encoderSlice = new BufferSlice(new byte[65535], 0, 65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, encoderSlice);
            var decoder = new MicroMessageDecoder(serializer) {MessageReceived = o => receivedMessage = o};
            var expected = "Hello world";
            var encoderArgs = new SocketBufferFake();
            
            encoder.Prepare(expected);
            encoder.Send(encoderArgs);
            encoderArgs.BytesTransferred = encoderArgs.Count;
            decoder.ProcessReadBytes(encoderArgs);

            receivedMessage.Should().Be(expected);
        }

        [Fact]
        public void codec_a_stream()
        {
            object receivedMessage = null;
            var encoderSlice = new BufferSlice(new byte[65535], 0, 65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, encoderSlice);
            var decoder = new MicroMessageDecoder(serializer) {MessageReceived = o => receivedMessage = o};
            var expected = "Hello world";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(expected));
            stream.SetLength(expected.Length);
            var encoderArgs = new SocketBufferFake();

            encoder.Prepare(stream);
            encoder.Send(encoderArgs);
            encoderArgs.BytesTransferred = encoderArgs.Count;
            decoder.ProcessReadBytes(encoderArgs);

            var reader = new StreamReader((Stream) receivedMessage);
            var msg = reader.ReadToEnd();
            msg.Should().Be(expected);
        }

        [Fact]
        public void codec_a_byte_buffer()
        {
            object receivedMessage = null;
            var encoderSlice = new BufferSlice(new byte[65535], 0, 65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, encoderSlice);
            var decoder = new MicroMessageDecoder(serializer) { MessageReceived = o => receivedMessage = o };
            var expected = new byte[] {1, 2, 3, 4, 5, 6, 7};
            var encoderArgs = new SocketBufferFake();

            encoder.Prepare(expected);
            encoder.Send(encoderArgs);
            encoderArgs.BytesTransferred = encoderArgs.Count;
            decoder.ProcessReadBytes(encoderArgs);

            var ms = (MemoryStream) receivedMessage;
            var buf = ms.GetBuffer();
            expected.Should().BeSubsetOf(buf);
            ms.Length.Should().Be(expected.Length);
        }

        [Fact]
        public void codec_a_custom_type()
        {
            object receivedMessage = null;
            var encoderSlice = new BufferSlice(new byte[65535], 0, 65535);
            var serializer = new DataContractMessageSerializer();
            var encoder = new MicroMessageEncoder(serializer, encoderSlice);
            var decoder = new MicroMessageDecoder(serializer) { MessageReceived = o => receivedMessage = o };
            var expected = new CustomType {Name = "Arne"};
            var encoderArgs = new SocketBufferFake();

            encoder.Prepare(expected);
            encoder.Send(encoderArgs);
            encoderArgs.BytesTransferred = encoderArgs.Count;
            decoder.ProcessReadBytes(encoderArgs);

            receivedMessage.Should().BeOfType<CustomType>();
            receivedMessage.As<CustomType>().Name.Should().Be(expected.Name);
        }


        public class CustomType
        {
            public string Name { get; set; }
        }
    }
}
