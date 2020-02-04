using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.Tests.Net.Buffers;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class HttpEncoderTests
    {
        [Fact]
        public async Task request_in_its_simplest_form()
        {
            var frame = new HttpRequest("POST", "/", "HTTP/1.1");
            var expected = "POST / HTTP/1.1\r\nContent-Length: 0\r\n\r\n";
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var encoder = new HttpMessageEncoder(buffer);
            await encoder.EncodeAsync(frame, channel);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task request_with_body()
        {
            var frame = new HttpRequest("POST", "/?abc", "HTTP/1.1");
            frame.AddHeader("server", "Griffin.Networking");
            frame.AddHeader("X-Requested-With", "XHttpRequest");
            frame.ContentType = "text/plain";
            frame.Body = new MemoryStream(Encoding.ASCII.GetBytes("hello queue a"));
            var expected = "POST /?abc HTTP/1.1\r\nserver: Griffin.Networking\r\nX-Requested-With: XHttpRequest\r\nContent-Type: text/plain\r\nContent-Length: 13\r\n\r\nhello queue a";
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var encoder = new HttpMessageEncoder(buffer);
            await encoder.EncodeAsync(frame, channel);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task basic_response()
        {
            var frame = new HttpResponse(404, "Failed to find it dude", "HTTP/1.1");
            var expected = "HTTP/1.1 404 Failed to find it dude\r\n";
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var encoder = new HttpMessageEncoder(buffer);
            await encoder.EncodeAsync(frame, channel);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Substring(0,expected.Length).Should().Be(expected);
        }

        [Fact]
        public async Task response_with_body()
        {
            var frame = new HttpResponse(HttpStatusCode.NotFound, "Failed to find it dude", "HTTP/1.1");
            frame.AddHeader("X-Requested-With", "XHttpRequest");
            frame.ContentType = "text/plain";
            frame.Body = new MemoryStream(Encoding.ASCII.GetBytes("hello queue a"));
            var expected = string.Format("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nDate: {0}\r\nContent-Type: text/plain\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a",
                DateTime.UtcNow.ToString("R"));
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var encoder = new HttpMessageEncoder(buffer);
            await encoder.EncodeAsync(frame, channel);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task response_with_body_encoding()
        {
            var frame = new HttpResponse(HttpStatusCode.NotFound, "Failed to find it dude", "HTTP/1.1");
            frame.AddHeader("X-Requested-With", "XHttpRequest");
            frame.ContentType = "text/plain";
            frame.ContentCharset = Encoding.UTF8;
            frame.Body = new MemoryStream(Encoding.UTF8.GetBytes("hello queue a"));
            var expected = string.Format("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nDate: {0}\r\nContent-Type: text/plain;charset=utf-8\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a",
                DateTime.UtcNow.ToString("R"));
            var buffer = new StandAloneBuffer(65535);
            var channel = Substitute.For<IBinaryChannel>();

            var encoder = new HttpMessageEncoder(buffer);
            await encoder.EncodeAsync(frame, channel);
            var actual = Encoding.ASCII.GetString(buffer.Buffer, 0, buffer.Count);

            actual.Should().Be(expected);
        }

    }
}
