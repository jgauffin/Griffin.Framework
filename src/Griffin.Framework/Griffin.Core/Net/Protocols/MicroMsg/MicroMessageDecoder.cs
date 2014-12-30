using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    /// Decode messages encoded with <see cref="MicroMessageEncoder"/>.
    /// </summary>
    /// <remarks>
    /// <para>As <see cref="MicroMessageEncoder"/> can out-of-the-box send <c>Stream</c>-drived classes and <c>byte[]</c> arrays this class
    /// has to handle that too.
    /// </para>
    /// <para>
    /// Streams will always be either <c>MemoryStream</c> or <c>FileStream</c> depending of the content-length. Same things goes for messages
    /// which are sent as <c>byte[]</c> arrays. They will also be received as streams.
    /// </para>
    /// </remarks>
    public class MicroMessageDecoder : IMessageDecoder
    {
        private readonly IMessageSerializer _serializer;

        /// <summary>
        /// Protocol version
        /// </summary>
        public const byte Version = 1;
        /// <summary>
        /// Size of the fixed header: version (1), content length (4), type name length (1) = 8
        /// </summary>
        /// <remarks>
        /// The header size field is not included in the actual header count as it always have to be read to 
        /// get the actual header size.
        /// </remarks>
        public const int FixedHeaderLength = sizeof(ushort) + sizeof (byte) + sizeof (int) + sizeof (byte);
        private int _bytesLeftForCurrentState;
        private int _contentLength;
        private byte _protocolVersion;
        private int _typeLength;
        private int _bytesLeftInSocketBuffer;
        private byte[] _header = new byte[short.MaxValue];
        private int _headerOffset;
        private int _socketBufferOffset;
        private Func<ISocketBuffer, bool> _stateMethod;
        private Stream _contentStream = new MemoryStream();

        private string _contentName;
        private Action<object> _messageReceived;
        private short _headerSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroMessageDecoder" /> class.
        /// </summary>
        /// <param name="serializer">The serializer used to decode the message that is being transported with MicroMsg.</param>
        /// <exception cref="System.ArgumentNullException">serializer</exception>
        public MicroMessageDecoder(IMessageSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;
            _bytesLeftForCurrentState = sizeof(short);
            _stateMethod = ReadHeaderLength;
        }

        /// <summary>
        /// Reset the decoder so that we can parse a new message
        /// </summary>
        public void Clear()
        {
            _bytesLeftForCurrentState = sizeof(short);
            _bytesLeftInSocketBuffer = 0;
            _contentLength = 0;
            _contentName = "";
            _contentStream.Close();
            _headerOffset = 0;
            _socketBufferOffset = 0;
            _stateMethod = ReadHeaderLength;
        }

        /// <summary>
        /// A new message have been received.
        /// </summary>
        /// <remarks>
        /// <para>The message will be a deserialized message or a <c>Stream</c> derived object (if the sender sent a <c>Stream</c> or a <c>byte[]</c> array).</para>
        /// </remarks>
        public Action<object> MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    value = o => {};

                _messageReceived = value;
            }
        }

        private bool ReadHeaderLength(ISocketBuffer e)
        {
            if (!CopyBytes(e))
                return false;

            _headerSize = BitConverter.ToInt16(_header, 0);
            _bytesLeftForCurrentState = _headerSize;
            _stateMethod = ProcessFixedHeader;
            _headerOffset = 0;
            return true;
        }

        private bool CopyBytes(ISocketBuffer e)
        {
            if (_bytesLeftInSocketBuffer == 0)
                return false;

            if (_bytesLeftForCurrentState > 0)
            {
                var toCopy = Math.Min(_bytesLeftForCurrentState, _bytesLeftInSocketBuffer);
                Buffer.BlockCopy(e.Buffer, _socketBufferOffset, _header, _headerOffset, toCopy);
                _headerOffset += toCopy;
                _bytesLeftForCurrentState -= toCopy;
                _bytesLeftInSocketBuffer -= toCopy;
                _socketBufferOffset += toCopy;
            }

            return _bytesLeftForCurrentState == 0;
        }

        private bool ProcessFixedHeader(ISocketBuffer e)
        {
            if (!CopyBytes(e))
                return false;

            _protocolVersion = _header[0];
            _contentLength = BitConverter.ToInt32(_header, 1);
            _typeLength = _header[5];
            _contentName = Encoding.ASCII.GetString(_header, 6, _typeLength);

            _stateMethod = ProcessContent;
            _bytesLeftForCurrentState = _contentLength;
            _headerOffset = 0;
            _contentStream.SetLength(0);
            _contentStream.Position = 0;
            return true;
        }

        private bool ProcessContent(ISocketBuffer arg)
        {
            if (_bytesLeftForCurrentState == 0 || _bytesLeftInSocketBuffer == 0)
                return false;

            var bytesToCopy = Math.Min(_bytesLeftForCurrentState, _bytesLeftInSocketBuffer);
            _contentStream.Write(arg.Buffer, _socketBufferOffset, bytesToCopy);
            _bytesLeftInSocketBuffer -= bytesToCopy;
            _bytesLeftForCurrentState -= bytesToCopy;
            _socketBufferOffset += bytesToCopy;

            if (_bytesLeftForCurrentState > 0)
            {
                return false;
            }

            _bytesLeftForCurrentState = sizeof(short);
            _headerOffset = 0;
            _stateMethod = ReadHeaderLength;
            _contentStream.Position = 0;

            
            if (_contentName == "stream")
                MessageReceived(_contentStream);
            else if (_contentName == "byte[]")
            {
                MessageReceived(_contentStream);
            }
            else
            {
                var message = _serializer.Deserialize(_contentName, _contentStream);
                MessageReceived(message);
            }

            return true;
        }

        /// <summary>
        /// Process bytes that we've received on the socket.
        /// </summary>
        /// <param name="buffer">Buffer to process.</param>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            _bytesLeftInSocketBuffer = buffer.BytesTransferred;
            _socketBufferOffset = buffer.Offset;

           
            while (_stateMethod(buffer))
            {
              
            }
        }
    }
}