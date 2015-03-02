using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Griffin.Net.Protocols.Http;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class HttpMessageDecoderTests
    {
        [Fact]
        public void request_with_a_multiline_header()
        {
            IHttpRequest actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
host: www.onetrueerror.com
Content-Length: 0
Multi-part: header
   which should be merged

");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpRequest)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("GET");
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/");
            actual.Headers["Multi-part"].Should().Be("header which should be merged");
            actual.Headers["content-length"].Should().Be("0");
        }

        [Fact]
        public void request_with_body()
        {
            IHttpRequest actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpRequest)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("PUT");
            actual.HttpVersion.Should().Be("HTTP/1.0");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["host"].Should().Be("www.onetrueerror.com");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }


        [Fact]
        public void decode_two_messages()
        {
            var actual = new List<IHttpRequest>();
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue aGET /?query HTTP/1.1
host: www.onetrueerror.com
content-length:14

hello queue aa");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual.Add((IHttpRequest)o);
            decoder.ProcessReadBytes(buffer);

            actual.Count.Should().Be(2);
            actual[1].HttpMethod.Should().Be("GET");
            actual[1].HttpVersion.Should().Be("HTTP/1.1");
            actual[1].Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual[1].Body.Should().NotBeNull();
            actual[1].Body.Length.Should().Be(14);
            actual[1].Headers["host"].Should().Be("www.onetrueerror.com");
            actual[1].Headers["content-length"].Should().Be("14");
            var sw = new StreamReader(actual[1].Body);
            sw.ReadToEnd().Should().Be("hello queue aa");
        }

        [Fact]
        public void decode_two_halves_where_the_body_is_partial()
        {
            IHttpRequest actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpRequest)o;

            buffer.BytesTransferred = buffer.Buffer.Length - 10;
            decoder.ProcessReadBytes(buffer);
            buffer.Offset = buffer.BytesTransferred;
            buffer.BytesTransferred = 10;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("PUT");
            actual.HttpVersion.Should().Be("HTTP/1.0");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["host"].Should().Be("www.onetrueerror.com");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void decode_two_halves_where_the_header_is_partial()
        {
            IHttpRequest actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpRequest)o;

            buffer.BytesTransferred = 10;
            decoder.ProcessReadBytes(buffer);
            buffer.Offset = 10;
            buffer.BytesTransferred = buffer.Buffer.Length - 10;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("PUT");
            actual.HttpVersion.Should().Be("HTTP/1.0");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(13);
            actual.Headers["host"].Should().Be("www.onetrueerror.com");
            actual.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void header_only_message_sprinkled_with_a_litte_bit_of_keepalive_lines()
        {
            IHttpRequest actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"



PUT / HTTP/1.0
host: www.onetrueerror.com
X-identity: 1

");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpRequest)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("PUT");
            actual.HttpVersion.Should().Be("HTTP/1.0");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/");
            actual.Body.Should().BeNull();
            actual.Headers["host"].Should().Be("www.onetrueerror.com");
            actual.Headers["X-identity"].Should().Be("1");
        }

        [Fact]
        public void header_only_message_sprinkled_with_a_litte_bit_of_NoOp_lines__and_finally_a_regular_message()
        {
            var actual = new List<IHttpRequest>();
            var buffer = new SocketBufferFake();
            buffer.Buffer =
                Encoding.ASCII.GetBytes(
                    @"


PUT / HTTP/1.0
host: www.onetrueerror.com
X-identity: 1


GET /?query HTTP/1.1
host: www.onetrueerror.com
content-length:13

hello queue a");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual.Add((IHttpRequest)o);
            decoder.ProcessReadBytes(buffer);

            actual.Count.Should().Be(2);
            actual[0].HttpMethod.Should().Be("PUT");
            actual[0].HttpVersion.Should().Be("HTTP/1.0");
            actual[0].Uri.ToString().Should().Be("http://www.onetrueerror.com/");
            actual[0].Body.Should().BeNull();
            actual[0].Headers["host"].Should().Be("www.onetrueerror.com");
            actual[0].Headers["X-identity"].Should().Be("1");
            actual[1].HttpMethod.Should().Be("GET");
            actual[1].HttpVersion.Should().Be("HTTP/1.1");
            actual[1].Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual[1].Body.Should().NotBeNull();
            actual[1].Body.Length.Should().Be(13);
            actual[1].Headers["host"].Should().Be("www.onetrueerror.com");
            actual[1].Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual[1].Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void basic_response()
        {
            IHttpResponse actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\n\r\n");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpResponse)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(404);
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.ReasonPhrase.Should().Be("Failed to find it dude");
            actual.Headers["Server"].Should().Be("griffinframework.net");
        }

        [Fact]
        public void response_with_body()
        {
            IHttpResponse actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nContent-Type: text/plain\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a\r\n\r\n");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpResponse)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(404);
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.ReasonPhrase.Should().Be("Failed to find it dude");
            actual.Headers["Server"].Should().Be("griffinframework.net");
            actual.Headers["X-Requested-With"].Should().Be("XHttpRequest");
            actual.ContentType.Should().Be("text/plain");
            actual.ContentLength.Should().Be(13);
            new StreamReader(actual.Body).ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void response_with_body_encoding()
        {
            IHttpResponse actual = null;
            var buffer = new SocketBufferFake();
            buffer.Buffer = Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nContent-Type: text/plain;charset=utf-8\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a\r\n\r\n");
            buffer.BytesTransferred = buffer.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual = (IHttpResponse)o;
            decoder.ProcessReadBytes(buffer);

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(404);
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.ReasonPhrase.Should().Be("Failed to find it dude");
            actual.Headers["Server"].Should().Be("griffinframework.net");
            actual.Headers["X-Requested-With"].Should().Be("XHttpRequest");
            actual.ContentType.Should().Be("text/plain");
            actual.ContentLength.Should().Be(13);
            actual.ContentCharset.Should().Be(Encoding.UTF8);
            new StreamReader(actual.Body, actual.ContentCharset).ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public void header_parser_should_be_reset_when_the_decoder_is_reset()
        {
            var actual = new List<IHttpRequest>();
            var buffer1 = new SocketBufferFake();
            buffer1.Buffer = Encoding.ASCII.GetBytes("GET / invalid_request1\r\n");
            buffer1.BytesTransferred = buffer1.Buffer.Length;

            var buffer2 = new SocketBufferFake();
            buffer2.Buffer = Encoding.ASCII.GetBytes("GET / invalid_request2\r\n");
            buffer2.BytesTransferred = buffer2.Buffer.Length;

            var decoder = new HttpMessageDecoder();
            decoder.MessageReceived = o => actual.Add((IHttpRequest)o);

            var ex1 = Assert.Throws<BadRequestException>(() => decoder.ProcessReadBytes(buffer1));
            ex1.Message.Should().Contain("'invalid_request1'");

            decoder.Clear();

            var ex2 = Assert.Throws<BadRequestException>(() => decoder.ProcessReadBytes(buffer2));
            ex2.Message.Should().Contain("'invalid_request2'");

            actual.Count.Should().Be(0);
        }  
    }
}