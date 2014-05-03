using System.IO;
using System.Text;
using Xunit;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Core.Tests.Net.Protocols.Http.Messages
{
    public class TextReaderExtensionsTests
    {
        [Fact]
        public void ReadToEnd_NoDelimiterHit()
        {
            var reader = GetReader("Hello world");

            var result = reader.ReadToEnd("&=");

            Assert.Equal("Hello world", result.Value);
            Assert.Equal(char.MinValue, result.Delimiter);
        }

        [Fact]
        public void ReadToEnd_DelimiterHit()
        {
            var reader = GetReader("Hello&World!");

            var result = reader.ReadToEnd("&!");

            Assert.Equal("Hello", result.Value);
            Assert.Equal('&', result.Delimiter);
        }

        [Fact]
        public void ReadToEnd_NoData()
        {
            var reader = GetReader("");

            var result = reader.ReadToEnd("&!");

            Assert.Equal("", result.Value);
            Assert.Equal(char.MinValue, result.Delimiter);
        }

        [Fact]
        public void ReadToEnd_ReadBothSectionsEndDelimiter()
        {
            var reader = GetReader("Hello&World!");

            var result1 = reader.ReadToEnd("&!");
            var result2 = reader.ReadToEnd("&!");

            Assert.Equal("Hello", result1.Value);
            Assert.Equal('&', result1.Delimiter);
            Assert.Equal("World", result2.Value);
            Assert.Equal('!', result2.Delimiter);
        }

        [Fact]
        public void ReadToEnd_ReadBothSections()
        {
            var reader = GetReader("Hello&World");

            var result1 = reader.ReadToEnd("&!");
            var result2 = reader.ReadToEnd("&!");

            Assert.Equal("Hello", result1.Value);
            Assert.Equal('&', result1.Delimiter);
            Assert.Equal("World", result2.Value);
            Assert.Equal(char.MinValue, result2.Delimiter);
        }


        private static StreamReader GetReader(string text)
        {
            var stream = new MemoryStream();
            var buffer = Encoding.ASCII.GetBytes(text);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;
            var reader = new StreamReader(stream);
            return reader;
        }
    }
}