using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Parses query string
    /// </summary>
    public class UrlDecoder
    {
        /// <summary>
        /// Parse a query string
        /// </summary>
        /// <param name="reader">string to parse</param>
        /// <param name="parameters">Parameter collection to fill</param>
        /// <returns>A collection</returns>
        /// <exception cref="ArgumentNullException"><c>reader</c> is <c>null</c>.</exception>
        public void Parse(TextReader reader, ParameterCollection parameters)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            var canRun = true;
            while (canRun)
            {
                var result = reader.ReadToEnd("&=");
                var name = WebUtility.UrlDecode(result.Value);
                switch (result.Delimiter)
                {
                    case '&':
                        parameters.Add(name, string.Empty);
                        break;
                    case '=':
                        result = reader.ReadToEnd("&");
                        parameters.Add(name, WebUtility.UrlDecode(result.Value));
                        break;
                    case char.MinValue:
                        // EOF = no delimiter && no value
                        if (!string.IsNullOrEmpty(name))
                            parameters.Add(name, string.Empty);
                        break;
                }

                canRun = result.Delimiter != char.MinValue;
            }
        }

        /// <summary>
        /// Parse a query string
        /// </summary>
        /// <param name="queryString">string to parse</param>
        /// <returns>A collection</returns>
        /// <exception cref="ArgumentNullException"><c>queryString</c> is <c>null</c>.</exception>
        public ParameterCollection Parse(string queryString)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");
            if (queryString.Length == 0)
                return new ParameterCollection();

            var reader = new StringReader(queryString);
            var col = new ParameterCollection();
            Parse(reader, col);
            return col;
        }
    }
}
