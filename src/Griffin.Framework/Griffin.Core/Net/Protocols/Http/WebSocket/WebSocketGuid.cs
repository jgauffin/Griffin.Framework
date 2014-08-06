using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketGuid
    {
        public const string ValueStr = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        public static readonly Guid Value = Guid.Parse(ValueStr);

        public string Hash(string webSocketKey)
        {
            if (webSocketKey == null) throw new ArgumentNullException("webSocketKey");
            var sha1 = SHA1.Create();
            sha1.Initialize();
            var stringToHash = Encoding.ASCII.GetBytes(webSocketKey + ValueStr);
            var result = sha1.ComputeHash(stringToHash);
            return Convert.ToBase64String(result);
        }
    }
}
