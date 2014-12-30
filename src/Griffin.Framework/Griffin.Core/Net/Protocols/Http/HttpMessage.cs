using System;
using System.Globalization;
using System.IO;
using System.Text;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Base class for HTTP requests and responses.
    /// </summary>
    public abstract class HttpMessage : IHttpMessage
    {
        /// <summary>
        ///     Used to be able to send responses in the same order as the request came in (if the web browser supports request
        ///     pipelining)
        /// </summary>
        public const string PipelineIndexKey = "X-Pipeline-Index";

        /// <summary>
        /// 8559-1 encoding
        /// </summary>
        protected static Encoding Iso85591 = Encoding.GetEncoding("ISO-8859-1");

        private readonly HeaderCollection _headers;
        private Stream _body;

        /// <summary>
        /// </summary>
        /// <param name="httpVersion">Version like <c>HTTP/1.1</c></param>
        protected HttpMessage(string httpVersion)
        {
            HttpVersion = httpVersion;
            _headers = new HeaderCollection(OnHeaderSet);
        }

        /// <summary>
        ///     Status line in a HTTP message divided into parts (array with three items).
        /// </summary>
        /// <remarks>
        ///     <para>The content of the line depends on if this is a HTTP request </para>
        /// </remarks>
        public abstract string StatusLine { get; }

        /// <summary>
        ///     Version of the HTTP protocol
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Typically <c>HTTP/1.1</c> but can also be the old version <c>HTTP/1.0</c> or the new draft <c>HTTP/2.0</c>
        ///         (aka SPDY)
        ///     </para>
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

        /// <summary>
        /// All HTTP headers.
        /// </summary>
        /// <remarks>
        /// Missing headers will return <c>null</c> as value
        /// </remarks>
        public IHeaderCollection Headers
        {
            get { return _headers; }
        }

        /// <summary>
        ///     Body in the HTTP message
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The body has not been modified or parsed in any way. The actual stream is either a <c>MemoryStream</c> or
        ///         <c>FileStream</c> depending on the
        ///         size of the body.
        ///     </para>
        /// </remarks>
        public Stream Body
        {
            get { return _body; }
            set
            {
                _body = value;
                if (!_headers.Contains("Content-Length"))
                    _headers["Content-Length"] = _body.Length.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        ///     Length of the body in bytes.
        /// </summary>
        public int ContentLength
        {
            get
            {
                if (_headers.Contains("Content-Length"))
                    return int.Parse(_headers["Content-Length"]);

                if (Body != null)
                    return (int) Body.Length;

                return 0;
            }
            set { _headers["Content-Length"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>
        ///     Content type without encoding
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         To set encoding you have to use <c>httpMessage.Headers["content-type"] = "text/html; charset=utf8"</c> or
        ///         similar.
        ///     </para>
        /// </remarks>
        public string ContentType
        {
            get
            {
                var str = _headers["Content-Type"];
                if (str == null)
                {
                    return "text/html";
                }

                var pos = str.IndexOf(';');
                return pos == -1 ? str : str.Substring(0, pos);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                var str = _headers["Content-Type"];
                if (str == null)
                {
                    _headers["Content-Type"] = value;
                    return;
                }

                var pos = str.IndexOf(';');
                if (pos == -1)
                    _headers["Content-Type"] = value;
                else
                    _headers["Content-Type"] = value + str.Substring(pos);
            }
        }

        /// <summary>
        ///     The encoding used in the document (if it's text of some sort)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         To set encoding you have to use <c>httpMessage.Headers["content-type"] = "text/html; charset=utf8"</c> or
        ///         similar.
        ///     </para>
        /// </remarks>
        public Encoding ContentCharset
        {
            get
            {
                var header = _headers["Content-Type"];
                if (header == null)
                    return Iso85591;

                var value = new HttpHeaderValue(header);
                var name = value.Parameters["charset"];
                return name == null ? Iso85591 : Encoding.GetEncoding(name);
            }
            set
            {
                var header = _headers["Content-Type"];
                if (header == null)
                {
                    _headers["Content-Type"] = "text/html;charset=" + value.WebName;
                    return;
                }

                var headerValue = new HttpHeaderValue(header);
                headerValue.Parameters.Add("charset", value.WebName);
                _headers["Content-Type"] = headerValue.ToString();
            }
        }

        /// <summary>
        ///     Invoked every time a HTTP header is modified.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Value</param>
        /// <remarks>
        ///     <para>Allows you to validate headers or modify the request when a specific header is set.</para>
        /// </remarks>
        protected virtual void OnHeaderSet(string name, string value)
        {
        }
    }
}