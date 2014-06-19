using System.IO;
using System.Text;
using Griffin.Net.Protocols.Http.Messages;
using Griffin.Net.Protocols.Http.Serializers;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http.BodyDecoders
{
    public class UrlDecoderTests
    {
        [Fact]
        public void TwoSimplePairs()
        {
            var actual = @"jonas=king&sara=queen";

            var decoder = new UrlDecoder();
            var result = decoder.Parse(actual);

            Assert.Equal("king", result["jonas"]);
            Assert.Equal("queen", result["sara"]);
        }

        [Fact]
        public void EncodedString()
        {
            var actual = @"jonas=king&sara=queen%26wife%20hmmz!";

            var decoder = new UrlDecoder();
            var result = decoder.Parse(actual);

            Assert.Equal("king", result["jonas"]);
            Assert.Equal("queen&wife hmmz!", result["sara"]);
        }

        [Fact]
        public void MultipleValuesUseLast()
        {
            var actual = @"jonas=king&sara=queen&sara=wife";

            var decoder = new UrlDecoder();
            var result = decoder.Parse(actual);

            Assert.Equal("wife", result["sara"]);
        }

        [Fact]
        public void TwoValuesCheckBoth()
        {
            var actual = @"jonas=king&sara=queen&sara=wife";

            var decoder = new UrlDecoder();
            var result = decoder.Parse(actual);

            Assert.Equal("queen", result.Get("sara")[0]);
            Assert.Equal("wife", result.Get("sara")[1]);
        }

        [Fact]
        public void Decode()
        {
            var contentType = "application/x-www-form-urlencoded;charset=ASCII";
            var body = new MemoryStream();
            body.Write(Encoding.ASCII.GetBytes("hello=world"), 0, "hello=world".Length);
            body.Position = 0;

            var decoder = new UrlFormattedMessageSerializer();
            var result = decoder.Deserialize(contentType, body);

            Assert.Equal("world", ((FormAndFilesResult) result).Form["hello"]);
        }
    }
}
