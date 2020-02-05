using System;
using System.Net;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     A HTTP response with minimal parsing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The purpose of this class is to do as little as possible with the response to make the processing more
    ///         straightforward and without
    ///         any unnecessary steps.
    ///     </para>
    /// </remarks>
    public class HttpResponse : HttpMessage
    {
        private HttpCookieCollection<HttpResponseCookie> _cookies;
        private string _reasonPhrase;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpResponse" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonPhrase">The reason phrase.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <exception cref="System.ArgumentNullException">reasonPhrase</exception>
        internal HttpResponse(int statusCode, string reasonPhrase, string httpVersion) : base(httpVersion)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase ?? throw new ArgumentNullException(nameof(reasonPhrase));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpResponse" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonPhrase">The reason phrase.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <exception cref="System.ArgumentNullException">reasonPhrase</exception>
        public HttpResponse(HttpStatusCode statusCode, string reasonPhrase, string httpVersion) : base(httpVersion)
        {
            if (reasonPhrase == null) throw new ArgumentNullException("reasonPhrase");
            StatusCode = (int) statusCode;
            ReasonPhrase = reasonPhrase;
            Headers["Server"] = "griffinframework.net";
            Headers["Date"] = DateTime.UtcNow.ToString("R");
        }

        /// <summary>
        ///     Cookies to send to the server side
        /// </summary>
        public HttpCookieCollection<HttpResponseCookie> Cookies
        {
            get => _cookies ?? (_cookies = new HttpCookieCollection<HttpResponseCookie>());
            set => _cookies = value;
        }


        /// <summary>
        ///     Why the specified <see cref="StatusCode" /> was set.
        /// </summary>
        public string ReasonPhrase
        {
            get => _reasonPhrase;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _reasonPhrase = value;
            }
        }

        /// <summary>
        ///     HTTP status code. You typically choose one of <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Status line for HTTP responses is "HttpVersion StatusCode ReasonPhrase"
        /// </summary>
        public override string StatusLine => HttpVersion + " " + StatusCode + " " + ReasonPhrase;
    }
}