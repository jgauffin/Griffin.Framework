using System;
using System.IO;
using System.Text;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    ///     Takes any object that the serializer supports and transfers it over the wire.
    /// </summary>
    /// <remarks>
    /// The encoder also natively supports <c>byte[]</c> arrays and <c>Stream</c> derived objects (as long as the stream have a size specified). These objects
    /// will be transferred without invoking the serializer.
    /// </remarks>
    public class MicroMessageEncoder : IMessageEncoder
    {
        public const byte Version = MicroMessageDecoder.Version;
        /// <summary>
        /// Size of the fixed header: version (1), content length (4), type name length (1) = 8
        /// </summary>
        /// <remarks>
        /// The header size field is not included in the actual header count as it always have to be read to 
        /// get the actual header size.
        /// </remarks>
        public const int FixedHeaderLength = MicroMessageDecoder.FixedHeaderLength;
        private readonly MemoryStream _internalStream = new MemoryStream();
        private readonly IMessageSerializer _serializer;
        private readonly IBufferSlice _bufferSlice;
        private Stream _bodyStream;
        private int _bytesEnqueued;
        private int _bytesLeftToSend;
        private int _bytesTransferred;
        private bool _headerIsSent;
        private object _message;
        private int _headerSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageEncoder" /> class.
        /// </summary>
        /// <param name="serializer">Serialiser used to serialize the messages that should be sent. You might want to pick a serializer which is reasonable fast.</param>
        public MicroMessageEncoder(IMessageSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");

            _serializer = serializer;
            _bufferSlice = new BufferSlice(new byte[65535], 0, 65535);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageEncoder" /> class.
        /// </summary>
        /// <param name="serializer">Serialiser used to serialize the messages that should be sent. You might want to pick a serializer which is reasonable fast.</param>
        /// <param name="bufferSlice">Used when sending information.</param>
        /// <exception cref="ArgumentOutOfRangeException">bufferSlice; At least the header should fit in the buffer, and the header can be up to 520 bytes in the current version.</exception>
        public MicroMessageEncoder(IMessageSerializer serializer, IBufferSlice bufferSlice)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (bufferSlice == null) throw new ArgumentNullException("bufferSlice");
            if (bufferSlice.Capacity < 520)
                throw new ArgumentOutOfRangeException("bufferSlice", bufferSlice.Capacity,
                                                      "At least the header should fit in the buffer, and the header can be up to 520 bytes in the current version");


            _serializer = serializer;
            _bufferSlice = bufferSlice;
        }


        /// <summary>
        ///     Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _headerIsSent = false;
        }

        /// <summary>
        /// Serialize message and sent it add it to the buffer
        /// </summary>
        /// <param name="args">Socket buffer</param>
        public void Send(ISocketBuffer args)
        {
            if (_bytesTransferred < _bytesEnqueued)
            {
                //TODO: Is this faster than moving the bytes to the beginning of the buffer and append more bytes?
                args.SetBuffer(_bufferSlice.Buffer, _bufferSlice.Offset + _bytesTransferred, _bytesEnqueued - _bytesTransferred);
                return;
            }

            if (!_headerIsSent)
            {
                var headerLength = CreateHeader();
                var bytesToWrite = (int)Math.Min(_bufferSlice.Capacity - headerLength, _bodyStream.Length);
                _bodyStream.Read(_bufferSlice.Buffer, _bufferSlice.Offset + headerLength, bytesToWrite);
                args.SetBuffer(_bufferSlice.Buffer, _bufferSlice.Offset, bytesToWrite + headerLength);
                _bytesEnqueued = headerLength + bytesToWrite;
                _bytesLeftToSend = headerLength + (int)_bodyStream.Length;
            }
            else
            {
                _bytesEnqueued = (int)Math.Min(_bufferSlice.Capacity, _bytesLeftToSend);
                _bodyStream.Write(_bufferSlice.Buffer, _bufferSlice.Offset, _bytesEnqueued);
                args.SetBuffer(_bufferSlice.Buffer, _bufferSlice.Offset, _bytesEnqueued);
            }
        }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks>
        ///     <c>true</c> if the message have been sent successfully; otherwise <c>false</c>.
        /// </remarks>
        public bool OnSendCompleted(int bytesTransferred)
        {
            // Make sure that the header is sent
            // required so that the Send() method can switch to the body state.
            if (!_headerIsSent)
            {
                _headerSize -= bytesTransferred;
                if (_headerSize <= 0)
                {
                    _headerIsSent = true;
                    _headerSize = 0;
                }
            }

            _bytesTransferred = bytesTransferred;
            _bytesLeftToSend -= bytesTransferred;
            if (_bytesLeftToSend == 0)
                Clear();

            return _bytesLeftToSend == 0;
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            _bytesEnqueued = 0;
            _bytesTransferred = 0;
            _bytesLeftToSend = 0;

            if (!ReferenceEquals(_bodyStream, _internalStream))
            {
                _bodyStream.Close();
                _bodyStream = null;
            }
            else
                _internalStream.SetLength(0);

            _headerIsSent = false;
            _headerSize = 0;
            _message = null;
        }

        private int CreateHeader()
        {
            string contentType;

            if (_message is Stream)
            {
                _bodyStream = (Stream)_message;
                contentType = "stream";
            }
            else if (_message is byte[])
            {
                var buffer = (byte[])_message;
                _bodyStream = new MemoryStream(buffer);
                _bodyStream.SetLength(buffer.Length);
                contentType = "byte[]";
            }
            else
            {
                _bodyStream = _internalStream;
                _serializer.Serialize(_message, _bodyStream, out contentType);
                if (contentType == null)
                    contentType = _message.GetType().AssemblyQualifiedName;
                if (contentType.Length > byte.MaxValue)
                    throw new InvalidOperationException(
                        "The AssemblyQualifiedName (type name) may not be larger than 255 characters. Your type: " +
                        _message.GetType().AssemblyQualifiedName);
            }

            var sliceOffset = _bufferSlice.Offset;
            var sliceBuffer = _bufferSlice.Buffer;
            _bodyStream.Position = 0;
            _headerSize = FixedHeaderLength + contentType.Length;

            BitConverter2.GetBytes((ushort)_headerSize, sliceBuffer, sliceOffset);
            _bufferSlice.Buffer[sliceOffset + 2] = Version;
            BitConverter2.GetBytes((int)_bodyStream.Length, sliceBuffer, sliceOffset + 2 + 1);
            BitConverter2.GetBytes((byte)contentType.Length, sliceBuffer, sliceOffset + 2 + 1 + 4);
            Encoding.UTF8.GetBytes(contentType, 0, contentType.Length, sliceBuffer, sliceOffset + 2 + 1 + 4 + 1);

            // the header length field is not included in _headerSize as it's a header prefix.
            // hence the +2
            return _headerSize;
        }
    }
}
