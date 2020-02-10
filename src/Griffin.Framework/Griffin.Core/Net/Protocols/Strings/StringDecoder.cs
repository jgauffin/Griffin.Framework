using System;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Strings
{
    /// <summary>
    /// Decodes messages that are represented as strings (4 bytes length and the rest is the string)
    /// </summary>
    public class StringDecoder : IMessageDecoder
    {
        private byte[] _buffer = new byte[65535];
        private Encoding _encoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDecoder"/> class.
        /// </summary>
        public StringDecoder()
        {
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Text encoding to use.
        /// </summary>
        public Encoding Encoding
        {
            get => _encoding;
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
        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment buffer)
        {
            while (buffer.BytesLeft() < 5)
            {
                await buffer.ReceiveMore(channel);
            }

            var len = BitConverter.ToInt32(buffer.Buffer, buffer.Offset);
            buffer.Offset += 4;

            var bytesRemaining = buffer.Count - (buffer.Offset - buffer.StartOffset);
            if (bytesRemaining >= len)
            {
                var result= Encoding.GetString(buffer.Buffer, buffer.Offset, len);
                buffer.Offset += len;
                return result;
            }

            if (_buffer.Length < len)
                _buffer = new byte[len];

            var bytesLeft = len;
            while (true)
            {
                var toCopy = Math.Min(buffer.BytesLeft(), bytesLeft);
                Buffer.BlockCopy(buffer.Buffer, buffer.Offset, _buffer, len-bytesLeft, toCopy);
                bytesLeft -= toCopy;

                if (bytesLeft > 0)
                    break;

                buffer.Offset = buffer.StartOffset;
                buffer.Count = 0;
                await channel.ReceiveAsync(buffer);
            }

            return Encoding.GetString(_buffer, 0, len);
        }

        /// <summary>
        /// Reset decoder state.
        /// </summary>
        public void Clear()
        {
        }

    }
}