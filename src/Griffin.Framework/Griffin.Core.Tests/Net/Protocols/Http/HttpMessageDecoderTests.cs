using System.Collections.Generic;
using System.IO;
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
    public class HttpMessageDecoderTests
    {
        [Fact]
        public async Task request_with_a_multiline_header()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf1 = Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
host: coderr.io
Content-Length: 0
Multi-part: header
   which should be merged

");
            var buffer = new StandAloneBuffer(buf1, 0, buf1.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("GET");
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.Uri.ToString().Should().Be("http://coderr.io/");
            actual.Headers["Multi-part"].Should().Be("header which should be merged");
            actual.Headers["content-length"].Should().Be("0");
        }

        [Fact]
        public async Task request_with_body()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

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
        public async Task allow_text_plain_even_if_the_decoder_doesnt_support_it()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf = Encoding.UTF8.GetBytes(@"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-type: text/plain
content-length:13

hello queue a");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual.Body.Position.Should().Be(0);
        }

        [Fact]
        public async Task decode_two_messages()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf = Encoding.ASCII.GetBytes(
                @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue aGET /?query HTTP/1.1
host: www.onetrueerror.com
content-length:14

hello queue aa");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual1 = (HttpRequest)await sut.DecodeAsync(channel, buffer);
            var actual2 = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual1.HttpMethod.Should().Be("GET");
            actual1.HttpVersion.Should().Be("HTTP/1.1");
            actual1.Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual1.Body.Should().NotBeNull();
            actual1.Body.Length.Should().Be(14);
            actual1.Headers["host"].Should().Be("www.onetrueerror.com");
            actual1.Headers["content-length"].Should().Be("14");
            var sw = new StreamReader(actual1.Body);
            sw.ReadToEnd().Should().Be("hello queue aa");
        }

        [Fact]
        public async Task decode_two_halves_where_the_body_is_partial()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length - 20);
            channel.WhenForAnyArgs(x => x.ReceiveAsync(null))
                .Do(x =>
                {
                    x.Arg<IBufferSegment>().Offset = buf.Length - 20;
                    x.Arg<IBufferSegment>().Count = 20;
                });
            channel.ReceiveAsync(Arg.Any<IBufferSegment>())
                .Returns(20);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

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
        public async Task decode_two_halves_where_the_header_is_partial()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
                Encoding.ASCII.GetBytes(
                    @"PUT /?query HTTP/1.0
host: www.onetrueerror.com
content-length:13

hello queue a");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);
            channel.WhenForAnyArgs(x => x.ReceiveAsync(null))
                .Do(x =>
                {
                    x.Arg<IBufferSegment>().Offset = 10;
                    x.Arg<IBufferSegment>().Count = buf.Length - 10;
                });
            channel.ReceiveAsync(Arg.Any<IBufferSegment>())
                .Returns(10);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

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
        public async Task header_only_message_sprinkled_with_a_litte_bit_of_keepalive_lines()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
                Encoding.ASCII.GetBytes(
                    @"



PUT / HTTP/1.0
host: www.onetrueerror.com
X-identity: 1

");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.HttpMethod.Should().Be("PUT");
            actual.HttpVersion.Should().Be("HTTP/1.0");
            actual.Uri.ToString().Should().Be("http://www.onetrueerror.com/");
            actual.Body.Should().BeNull();
            actual.Headers["host"].Should().Be("www.onetrueerror.com");
            actual.Headers["X-identity"].Should().Be("1");
        }

        [Fact]
        public async Task header_only_message_sprinkled_with_a_litte_bit_of_NoOp_lines__and_finally_a_regular_message()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
                Encoding.ASCII.GetBytes(
                    @"


PUT / HTTP/1.0
host: www.onetrueerror.com
X-identity: 1


GET /?query HTTP/1.1
host: www.onetrueerror.com
content-length:13

hello queue a");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual0 = (HttpRequest)await sut.DecodeAsync(channel, buffer);
            var actual1 = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual0.HttpMethod.Should().Be("PUT");
            actual0.HttpVersion.Should().Be("HTTP/1.0");
            actual0.Uri.ToString().Should().Be("http://www.onetrueerror.com/");
            actual0.Body.Should().BeNull();
            actual0.Headers["host"].Should().Be("www.onetrueerror.com");
            actual0.Headers["X-identity"].Should().Be("1");
            actual1.HttpMethod.Should().Be("GET");
            actual1.HttpVersion.Should().Be("HTTP/1.1");
            actual1.Uri.ToString().Should().Be("http://www.onetrueerror.com/?query");
            actual1.Body.Should().NotBeNull();
            actual1.Body.Length.Should().Be(13);
            actual1.Headers["host"].Should().Be("www.onetrueerror.com");
            actual1.Headers["content-length"].Should().Be("13");
            var sw = new StreamReader(actual1.Body);
            sw.ReadToEnd().Should().Be("hello queue a");
        }

        [Fact]
        public async Task basic_response()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
            Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\n\r\n");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpResponse)await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(404);
            actual.HttpVersion.Should().Be("HTTP/1.1");
            actual.ReasonPhrase.Should().Be("Failed to find it dude");
            actual.Headers["Server"].Should().Be("griffinframework.net");
        }

        [Fact]
        public async Task response_with_body()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
            Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nContent-Type: text/plain\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a\r\n\r\n");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpResponse)await sut.DecodeAsync(channel, buffer);

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
        public async Task response_with_body_encoding()
        {
            var channel = Substitute.For<IBinaryChannel>();
            var buf =
            Encoding.ASCII.GetBytes("HTTP/1.1 404 Failed to find it dude\r\nServer: griffinframework.net\r\nContent-Type: text/plain;charset=utf-8\r\nX-Requested-With: XHttpRequest\r\nContent-Length: 13\r\n\r\nhello queue a\r\n\r\n");
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpResponse)await sut.DecodeAsync(channel, buffer);

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
        public async Task ParsePost()
        {
            var buf = Encoding.UTF8.GetBytes(@"POST / HTTP/1.1
Host: localhost:8080
Connection: keep-alive
Content-Length: 11
Origin: http://localhost:8080
X-Requested-With: XMLHttpRequest
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11
Content-Type: application/x-www-form-urlencoded; charset=UTF-32
Accept: */*
Referer: http://localhost:8080/ajaxPost.html
Accept-Encoding: gzip,deflate,sdch
Accept-Language: sv,en;q=0.8,en-US;q=0.6
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
Cookie: ASP.NET_SessionId=5vkr4tfivb1ybu1sm4u4kahy; GriffinLanguageSwitcher=sv-se; __RequestVerificationToken=LiTSJATsiqh8zlcft_3gZwvY8HpcCUkirm307njxIZLdsJSYyqaV2st1tunH8sMvMwsVrj3W4dDoV8ECZRhU4s6DhTvd2F-WFkgApDBB-CA1; .ASPXAUTH=BF8BE1C246428B10B49AE867BEDF9748DB3842285BC1AF1EC44AD80281C4AE084B75F0AE13EAF1BE7F71DD26D0CE69634E83C4846625DC7E4D976CA1845914E2CC7A7CF2C522EA5586623D9B73B0AE433337FC59CF6AF665DC135491E78978EF

hello=world");
            var channel = Substitute.For<IBinaryChannel>();
            var buffer = new StandAloneBuffer(buf, 00, buf.Length);

            var sut = new HttpMessageDecoder();
            var actual = (HttpRequest)await sut.DecodeAsync(channel, buffer);

            actual.Should().NotBeNull();
            actual.ContentCharset.Should().Be(Encoding.UTF32);
            actual.ContentLength.Should().Be(11);
            actual.Body.Should().NotBeNull();
            actual.Body.Position.Should().Be(0);
            actual.Body.Length.Should().Be(11);
        }

    }
}