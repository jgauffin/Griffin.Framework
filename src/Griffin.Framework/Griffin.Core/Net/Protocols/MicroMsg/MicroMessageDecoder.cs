using System;
using System.IO;
using System.Text;
using Griffin.Net.Channels;

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
        public const byte Version = 1;

        /// <summary>
        /// Size of the fixed header: (version, content length, type name length)
        /// </summary>
        public const int FixedHeaderLength = sizeof (byte) + sizeof (int) + sizeof (byte);
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

        public MicroMessageDecoder(IMessageSerializer serializer)
        {
            _serializer = serializer;
            _bytesLeftForCurrentState = sizeof(short);
            _stateMethod = ReadHeaderLength;
        }

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

        public bool ReadHeaderLength(ISocketBuffer e)
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

        public bool ProcessFixedHeader(ISocketBuffer e)
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

            var type = Type.GetType(_contentName);
            if (type != null && typeof (Stream).IsAssignableFrom(type))
                MessageReceived(_contentStream);
            else if (type == typeof (byte[]))
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