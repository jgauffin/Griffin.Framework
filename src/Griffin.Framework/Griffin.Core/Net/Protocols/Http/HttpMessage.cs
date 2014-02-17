using System;
using System.IO;
using System.Text;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    public abstract class HttpMessage : IHttpMessage
    {
        private HeaderCollection _headers;
        private Stream _body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpVersion">Version like <c>HTTP/1.1</c></param>
        protected HttpMessage(string httpVersion)
        {
            HttpVersion = httpVersion;
            _headers = new HeaderCollection(OnHeaderSet);
        }

        /// <summary>
        /// Invoked every time a HTTP header is modified.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Value</param>
        /// <remarks>
        /// <para>Allows you to validate headers or modify the request when a specific header is set.</para>
        /// </remarks>
        protected virtual void OnHeaderSet(string name, string value)
        {
        }

        /// <summary>
        /// Status line in a HTTP message divided into parts (array with three items).
        /// </summary>
        /// <remarks>
        /// <para>The content of the line depends on if this is a HTTP request </para>
        /// </remarks>
        public abstract string StatusLine { get; }

        /// <summary>
        /// Version of the HTTP protocol
        /// </summary>
        /// <remarks>
        /// <para>Typically <c>HTTP/1.1</c> but can also be the old version <c>HTTP/1.0</c> or the new draft <c>HTTP/2.0</c> (aka SPDY)</para>
        /// </remarks>
        public string HttpVersion { get; set; }

        /// <summary>
        ///     Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        /// <exception cref="System.FormatException">
        ///     Header name may not contain colon, CR or LF.
        ///     or
        ///     Header value may not contain colon, CR or LF.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         If a client or a server receives repeated frame header entries, only the first header entry SHOULD be used as
        ///         the value of header entry. Subsequent values are only used to maintain a history of state changes of the header
        ///         and MAY be ignored. This implementation will IGNORE all subsequent headers
        ///     </para>
        /// </remarks>
        public void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        public IHeaderCollection Headers
        {
            get { return _headers; }
        }

        /// <summary>
        /// Body in the HTTP message
        /// </summary>
        /// <remarks>
        /// <para>
        /// The body has not been modified or parsed in any way. The actual stream is either a <c>MemoryStream</c> or <c>FileStream</c> depending on the
        /// size of the body.
        /// </para>
        /// </remarks>
        public Stream Body
        {
            get { return _body; }
            set
            {
                _body = value;
                if (!_headers.Contains("content-length"))
                    _headers["content-length"] = _body.Length.ToString();
            }
        }

        /// <summary>
        /// Length of the body in bytes.
        /// </summary>
        public int ContentLength
        {
            get
            {
                if (_headers.Contains("content-length"))
                    return int.Parse(_headers["content-length"]);

                if (Body != null)
                    return (int)Body.Length;

                return 0;
            }
            set { _headers["content-length"] = value.ToString(); }
        }

        /// <summary>
        /// Content type without encoding
        /// </summary>
        /// <remarks>
        /// <para>To set encoding you have to use <c>httpMessage.Headers["content-type"] = "text/html; charset=utf8"</c> or similar.</para>
        /// </remarks>
        public string ContentType
        {
            get
            {
                return _headers["content-type"];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// The encoding used in the document (if it's text of some sort)
        /// </summary>
        /// <remarks>
        /// <para>To set encoding you have to use <c>httpMessage.Headers["content-type"] = "text/html; charset=utf8"</c> or similar.</para>
        /// </remarks>
        public Encoding ContentCharset
        {
            get
            {
                var header = _headers["content-type"];
                if (header == null)
                    return null;

                var pos = header.IndexOf(";", System.StringComparison.Ordinal);
                if (pos == -1)
                    return Encoding.UTF8;

                var encoding = header.Substring(pos + 1).Trim();
                return Encoding.GetEncoding(encoding);
            }
        }


    }
}
