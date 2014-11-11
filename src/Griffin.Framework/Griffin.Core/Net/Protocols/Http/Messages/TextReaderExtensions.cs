using System.IO;
using System.Text;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Extensions for the standard text reader
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        public static ReaderResult ReadToEnd(this TextReader reader, string delimiters)
        {
            var result = new ReaderResult();
            var builder = new StringBuilder();

            var intChar = reader.Read();
            while (intChar != -1 && delimiters.IndexOf((char)intChar) == -1)
            {
                builder.Append((char)intChar);
                intChar = reader.Read();
            }

            result.Value = builder.ToString();
            result.Delimiter = intChar == -1 ? char.MinValue : (char)intChar;
            return result;
        }
    }
}