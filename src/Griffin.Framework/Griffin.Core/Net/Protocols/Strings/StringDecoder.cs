using System;
using System.Text;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Strings
{
    /// <summary>
    /// Decodes messages that are represented as strings.
    /// </summary>
    public class StringDecoder : IMessageDecoder
    {
        private byte[] _buffer = new byte[65535];
        private int _bytesLeftForCurrentMsg = -1;
        private Encoding _encoding;
        private Action<object> _messageReceived;
        private int _msgLength;
        private int _offsetInOurBuffer;
        private bool _readHeader = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDecoder"/> class.
        /// </summary>
        public StringDecoder()
        {
            _bytesLeftForCurrentMsg = 4;
            _readHeader = true;
            _messageReceived = o => { };
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Text encoding to use.
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                if (value == null)
                    value = Encoding.UTF8;

                _encoding = value;
            }
        }

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            var offsetInSocketBuffer = buffer.Offset;
            var bytesLeftInSocketBuffer = buffer.BytesTransferred;

            while (bytesLeftInSocketBuffer > 0)
            {
                var len = Math.Min(bytesLeftInSocketBuffer, _bytesLeftForCurrentMsg);
                Buffer.BlockCopy(buffer.Buffer, offsetInSocketBuffer, _buffer, _offsetInOurBuffer, buffer.BytesTransferred);
                offsetInSocketBuffer += len;
                bytesLeftInSocketBuffer -= len;
                _bytesLeftForCurrentMsg -= len;
                _offsetInOurBuffer += len;

                if (_bytesLeftForCurrentMsg != 0)
                    continue;

                if (_readHeader)
                {
                    _offsetInOurBuffer = 0;
                    _msgLength = _bytesLeftForCurrentMsg = BitConverter.ToInt32(_buffer, 0);
                    _readHeader = false;
                }
                else
                {
                    _readHeader = true;
                    _bytesLeftForCurrentMsg = 4;
                    MessageReceived(Encoding.GetString(_buffer, 0, _msgLength));
                }
            }
        }

        /// <summary>
        /// Reset decoder state.
        /// </summary>
        public void Clear()
        {
            _offsetInOurBuffer = -1;
            _bytesLeftForCurrentMsg = -1;
        }

        /// <summary>
        ///     A message have been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    value = o => { };

                _messageReceived = value;
            }
        }
    }
}