using System.IO;
using System.Text;
using Griffin.Net.Protocols.Http.Serializers;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http.BodyDecoders
{
    public class MultipartSerializerTests
    {
        [Fact]
        public void DecodeFile()
        {
            var contentType = "multipart/form-data; boundary=AaB03x";
            var message = "--AaB03x\r\nContent-Disposition: form-data; name=\"hello\"\r\n\r\nworld\r\n--AaB03x\r\nContent-Disposition: form-data; name=\"file\"; filename=\"file.txt\"\r\nContent-Type: text/plain\r\n\r\nhello world\r\n--AaB03x--";
            var bytes = Encoding.ASCII.GetBytes(message);
            var body = new MemoryStream(bytes);

            var decoder = new MultipartSerializer();
            var result = decoder.Deserialize(contentType, body);

            Assert.Equal("world", ((FormAndFilesContent)result).Form["hello"]);
            Assert.Equal("file", ((FormAndFilesContent)result).Files["file"].Name);
            Assert.Equal("file.txt", ((FormAndFilesContent)result).Files["file"].OriginalFileName);
            Assert.Equal("text/plain", ((FormAndFilesContent)result).Files["file"].ContentType);
            Assert.Equal("hello world", File.ReadAllText(((FormAndFilesContent)result).Files["file"].TempFileName));
        }
    }
}
