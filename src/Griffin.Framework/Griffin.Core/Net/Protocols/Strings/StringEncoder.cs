using System;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Strings
{
    /// <summary>
    /// Encodes strings using UTF8 and sends them over the network with a binary header (<c>int</c>).
    /// </summary>
    public class StringEncoder : IMessageEncoder
    {
        private IBufferSegment _buffer;

        public StringEncoder(IBufferSegment buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public StringEncoder()
        {
            _buffer = new StandAloneBuffer(65535);
        }

        public async Task EncodeAsync(object message, IBinaryChannel channel)
        {
            var str = message.ToString();
            var len = Encoding.UTF8.GetByteCount(str);

            if (len > _buffer.Buffer.Length)
                _buffer = new StandAloneBuffer(len);

            BitConverter2.GetBytes(len, _buffer.Buffer, _buffer.StartOffset);
            Encoding.UTF8.GetBytes(str, 0, str.Length, _buffer.Buffer, _buffer.StartOffset + 4);
            _buffer.Count = len + 4;
            await channel.SendAsync(_buffer);
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
        }
    }
}