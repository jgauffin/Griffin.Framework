using System;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.Messages;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http.Messages
{
    public class HeaderParserTests
    {
        [Fact]
        public async Task Parse()
        {
            var buffer = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nSERVER: LOCALHOST\r\n\r\n");
            var slice = new StandAloneBuffer(buffer, 0, buffer.Length);
            var channel = Substitute.For<IBinaryChannel>();

            var parser = new HeaderParser();
            parser.HeaderParsed += (name, value) => Console.WriteLine(name + ": " + value);
            await parser.Parse(slice, channel);

        }

        [Fact]
        public async Task ParseHeader_SkipBody()
        {
            const string HttpPost = @"POST / HTTP/1.1
Host: localhost:8080
Connection: keep-alive
Content-Length: 11
Origin: http://localhost:8080
X-Requested-With: XMLHttpRequest
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
Accept: */*
Referer: http://localhost:8080/ajaxPost.html
Accept-Encoding: gzip,deflate,sdch
Accept-Language: sv,en;q=0.8,en-US;q=0.6
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
Cookie: ASP.NET_SessionId=5vkr4tfivb1ybu1sm4u4kahy; GriffinLanguageSwitcher=sv-se; __RequestVerificationToken=LiTSJATsiqh8zlcft_3gZwvY8HpcCUkirm307njxIZLdsJSYyqaV2st1tunH8sMvMwsVrj3W4dDoV8ECZRhU4s6DhTvd2F-WFkgApDBB-CA1; .ASPXAUTH=BF8BE1C246428B10B49AE867BEDF9748DB3842285BC1AF1EC44AD80281C4AE084B75F0AE13EAF1BE7F71DD26D0CE69634E83C4846625DC7E4D976CA1845914E2CC7A7CF2C522EA5586623D9B73B0AE433337FC59CF6AF665DC135491E78978EF

hello=world";
            string actual = "";
            var buffer = Encoding.UTF8.GetBytes(HttpPost);
            var slice = new StandAloneBuffer(buffer, 0, buffer.Length);
            var channel = Substitute.For<IBinaryChannel>();
            var parser = new HeaderParser();
            parser.HeaderParsed += (name, value) => actual = value;
            await parser.Parse(slice, channel);

            Assert.Equal("ASP.NET_SessionId=5vkr4tfivb1ybu1sm4u4kahy; GriffinLanguageSwitcher=sv-se; __RequestVerificationToken=LiTSJATsiqh8zlcft_3gZwvY8HpcCUkirm307njxIZLdsJSYyqaV2st1tunH8sMvMwsVrj3W4dDoV8ECZRhU4s6DhTvd2F-WFkgApDBB-CA1; .ASPXAUTH=BF8BE1C246428B10B49AE867BEDF9748DB3842285BC1AF1EC44AD80281C4AE084B75F0AE13EAF1BE7F71DD26D0CE69634E83C4846625DC7E4D976CA1845914E2CC7A7CF2C522EA5586623D9B73B0AE433337FC59CF6AF665DC135491E78978EF", actual);
            Assert.Equal('h', (char)slice.Buffer[slice.Offset]);
        }

    }
}