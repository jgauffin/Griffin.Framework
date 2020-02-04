using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    ///     Takes any object that the serializer supports and transfers it over the wire.
    /// </summary>
    /// <remarks>
    ///     The encoder also natively supports <c>byte[]</c> arrays and <c>Stream</c> derived objects (as long as the stream
    ///     have a size specified). These objects
    ///     will be transferred without invoking the serializer.
    /// </remarks>
    public class MicroMessageEncoder : IMessageEncoder
    {
        /// <summary>
        ///     PROTOCOL version
        /// </summary>
        public const byte Version = MicroMessageDecoder.Version;

        /// <summary>
        ///     Size of the fixed header: version (1), content length (4), type name length (1) = 8
        /// </summary>
        /// <remarks>
        ///     The header size field is not included in the actual header count as it always have to be read to
        ///     get the actual header size.
        /// </remarks>
        public const int FixedHeaderLength = MicroMessageDecoder.FixedHeaderLength;
        private readonly IBufferSegment _buffer;
        private readonly MemoryStream _internalStream = new MemoryStream();
        private readonly IMessageSerializer _serializer;
        private Stream _bodyStream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageEncoder" /> class.
        /// </summary>
        /// <param name="serializer">
        ///     Serializer used to serialize the messages that should be sent. You might want to pick a
        ///     serializer which is reasonable fast.
        /// </param>
        public MicroMessageEncoder(IMessageSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _buffer = new StandAloneBuffer(new byte[65535], 0, 65535);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageEncoder" /> class.
        /// </summary>
        /// <param name="serializer">
        ///     Serializer used to serialize the messages that should be sent. You might want to pick a
        ///     serializer which is reasonable fast.
        /// </param>
        /// <param name="buffer">Used when sending information.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     bufferSlice; At least the header should fit in the buffer, and the header
        ///     can be up to 520 bytes in the current version.
        /// </exception>
        public MicroMessageEncoder(IMessageSerializer serializer, IBufferSegment buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Capacity < 520)
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Capacity,
                    "At least the header should fit in the buffer, and the header can be up to 520 bytes in the current version");


            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _buffer = buffer;
        }

        /// <summary>
        ///     Serialize message and sent it add it to the buffer
        /// </summary>
        /// <param name="args">Socket buffer</param>
        public async Task EncodeAsync(object message, IBinaryChannel channel)
        {
            WriteHeaderToBuffer(_buffer, message);
            _buffer.Count = _buffer.Offset;
            _buffer.Offset = _buffer.StartOffset;
            if (_bodyStream is MemoryStream ms)
            {
                await channel.SendMoreAsync(_buffer);
                await channel.SendAsync(new StandAloneBuffer(ms.GetBuffer(), 0, (int)ms.Length));
            }
            else
            {
                await channel.SendAsync(_buffer);
                var bytesLeft = (int)_bodyStream.Length;
                while (bytesLeft > 0)
                {
                    _buffer.Count = await _bodyStream.ReadAsync(_buffer.Buffer, _buffer.StartOffset, _buffer.Capacity);
                    await channel.SendAsync(_buffer);
                    bytesLeft -= _buffer.Count;
                }
            }
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            if (!ReferenceEquals(_bodyStream, _internalStream))
            {
                //bodyStream is null for channels that connected
                //but never sent a message.
                _bodyStream?.Dispose();
                _bodyStream = null;
            }
            else
                _internalStream.SetLength(0);
        }

        private void WriteHeaderToBuffer(IBufferSegment buffer, object message)
        {
            string contentType;

            if (message is Stream stream)
            {
                _bodyStream = stream;
                contentType = "stream";
            }
            else if (message is byte[] buffer1)
            {
                //TODO: Send it directly using sendMore
                _bodyStream = new MemoryStream(buffer1);
                _bodyStream.SetLength(buffer1.Length);
                contentType = "byte[]";
            }
            else
            {
                _bodyStream = _internalStream;
                _serializer.Serialize(message, _bodyStream, out contentType);
                if (contentType == null)
                    contentType = message.GetType().AssemblyQualifiedName;
                if (contentType.Length > byte.MaxValue)
                    throw new InvalidOperationException(
                        "The AssemblyQualifiedName (type name) may not be larger than 255 characters. Your type: " +
                        message.GetType().AssemblyQualifiedName);
            }

            _bodyStream.Position = 0;
            var headerSize = FixedHeaderLength + contentType.Length;

            BitConverter2.GetBytes((ushort)headerSize, buffer.Buffer, buffer.Offset);
            buffer.Offset += 2;

            _buffer.Buffer[buffer.Offset++] = Version;

            BitConverter2.GetBytes((int)_bodyStream.Length, buffer.Buffer, buffer.Offset);
            buffer.Offset += 4;

            _buffer.Buffer[buffer.Offset++] = (byte)contentType.Length;

            var len = Encoding.UTF8.GetBytes(contentType, 0, contentType.Length, buffer.Buffer, buffer.Offset);
            buffer.Offset += len;
        }
    }
}