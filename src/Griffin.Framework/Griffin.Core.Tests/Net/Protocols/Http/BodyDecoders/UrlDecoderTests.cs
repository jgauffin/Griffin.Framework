using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.BodyDecoders;
using Griffin.Net.Protocols.Http.Messages;
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
            var request = new HttpRequest("POST", "/", "HTTP/1.1");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded;charset=ASCII");
            request.Body = new MemoryStream();
            request.Body.Write(Encoding.ASCII.GetBytes("hello=world"), 0, "hello=world".Length);
            request.Body.Position = 0;

            var decoder = new UrlFormattedDecoder();
            decoder.Decode(request);

            Assert.Equal("world", request.Form["hello"]);
        }
    }
}
