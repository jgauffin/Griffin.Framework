using System;

namespace Griffin.Net.Protocols.Http
{

    /// <summary>
    /// A HTTP response with minimal parsing.
    /// </summary>
    /// <remarks>
    /// <para>The purpose of this class is to do as little as possible with the response to make the processing more straightforward and without
    /// any unnessacary steps.</para>
    /// </remarks>
    public class BasicHttpResponse : HttpMessage, IHttpResponse
    {
        private string _reasonPhrase;
        private readonly HttpCookieCollection<IResponseCookie> _cookies = new HttpCookieCollection<IResponseCookie>();

        public BasicHttpResponse(int httpStatusCode, string reasonPhrase, string httpVersion) : base(httpVersion)
        {
            if (reasonPhrase == null) throw new ArgumentNullException("reasonPhrase");

            StatusCode = httpStatusCode;
            ReasonPhrase = reasonPhrase;
        }

        /// <inheritdoc />
        public IHttpCookieCollection<IResponseCookie> Cookies { get { return _cookies; } }

        /// <summary>
        ///     HTTP status code. You typically choose one of <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Why the specified <see cref="StatusCode" /> was set.
        /// </summary>
        public string ReasonPhrase
        {
            get { return _reasonPhrase; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _reasonPhrase = value;
            }
        }

        /// <summary>
        /// Status line for HTTP responses is "HttpVersion StatusCode ReasonPhrase"
        /// </summary>
        public override string StatusLine
        {
            get { return HttpVersion + " " + StatusCode + " " + ReasonPhrase; }
        }
    }
}