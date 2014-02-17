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

    }
}