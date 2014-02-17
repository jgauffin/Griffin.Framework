using System.IO;
using System.Text;
using Griffin.Net.Protocols.MicroMsg;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    public class StringSerializer : IMessageSerializer
    {
        public void Serialize(object source, Stream destination, out string contentType)
        {
            var buf = Encoding.ASCII.GetBytes((string)source);
            destination.Write(buf, 0, buf.Length);
            contentType = "string";
        }

        public object Deserialize(string contentType, Stream source)
        {
            var buf = new byte[source.Length];
            source.Read(buf, 0, (int)source.Length);
            return Encoding.ASCII.GetString(buf);
        }
    }
}