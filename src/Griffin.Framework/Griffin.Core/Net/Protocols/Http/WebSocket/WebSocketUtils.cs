using System;
using System.Security.Cryptography;
using System.Text;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    internal class WebSocketUtils
    {
        private const string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// Hashes the client WebSocket key for the server
        /// </summary>
        /// <param name="webSocketKey"></param>
        /// <returns></returns>
        public static string HashWebSocketKey(string webSocketKey)
        {
            if (webSocketKey == null) throw new ArgumentNullException("webSocketKey");
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(webSocketKey + _guid);
                var hash = sha1.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Creates a new random WebSocket key for the client
        /// </summary>
        /// <returns></returns>
        public static string CreateWebSocketKey()
        {
            var key = new byte[16];
            new Random().NextBytes(key);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Check if http message is a valid WebSocket upgrade request
        /// </summary>
        /// <param name="httpMessage">message to check</param>
        /// <returns>true if message is a valid WebSocket upgrade request</returns>
        public static bool IsWebSocketUpgrade(IHttpMessage httpMessage)
        {
            return httpMessage != null &&
                (httpMessage.Headers["Connection"] ?? string.Empty).IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) != -1 &&
                (httpMessage.Headers["Upgrade"] ?? string.Empty).IndexOf("websocket", StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>
        /// Creates a new radom masking key
        /// </summary>
        /// <returns>masking key</returns>
        public static byte[] CreateMaskingKey()
        {
            var key = new byte[4];
            new Random().NextBytes(key);
            return key;
        }

        /// <summary>
        /// Helper function to convert a byte array to a short using big endian
        /// </summary>
        /// <param name="value">byte array</param>
        /// <returns></returns>
        public static ushort ToBigEndianUInt16(byte[] value)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(value);
            return BitConverter.ToUInt16(value, 0);
        }

        /// <summary>
        /// Helper function to convert a byte array to a long using big endian
        /// </summary>
        /// <param name="value">byte array</param>
        /// <returns></returns>
        public static ulong ToBigEndianUInt64(byte[] value)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(value);
            return BitConverter.ToUInt64(value, 0);
        }

        /// <summary>
        /// Helper function to convert a short to a byte array using big endian
        /// </summary>
        /// <param name="value">short</param>
        /// <returns></returns>
        public static byte[] GetBigEndianBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Helper function to convert a long to a byte array using big endian
        /// </summary>
        /// <param name="value">long</param>
        /// <returns></returns>
        public static byte[] GetBigEndianBytes(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

    }
}
