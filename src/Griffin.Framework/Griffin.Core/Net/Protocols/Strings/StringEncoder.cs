using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Strings
{
    /// <summary>
    /// Encodes strings using UTF8 and sends them over the network with a binary header (<c>int</c>).
    /// </summary>
    public class StringEncoder : IMessageEncoder
    {
        private byte[] _buffer = new byte[65535];

        public async Task EncodeAsync(object message, IBinaryChannel channel)
        {
            var str = message.ToString();
            var len = Encoding.UTF8.GetByteCount(str);

            if (len > _buffer.Length)
                _buffer = new byte[len];

            BitConverter2.GetBytes(len, _buffer, 0);
            Encoding.UTF8.GetBytes(str, 0, str.Length, _buffer, 4);
            await channel.SendAsync(_buffer, 0, len + 4);
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
        }
    }
}