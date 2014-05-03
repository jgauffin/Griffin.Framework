using System.Net;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Complete HTTP resposne.
    /// </summary>
    public class HttpResponse : HttpResponseBase
    {
        public HttpResponse(int statusCode, string reasonPhrase, string httpVersion) : base(statusCode, reasonPhrase, httpVersion)
        {
            Cookies = new HttpCookieCollection<IResponseCookie>();
        }

        public HttpResponse(HttpStatusCode statusCode, string reasonPhrase, string httpVersion) : base(statusCode, reasonPhrase, httpVersion)
        {
            Cookies = new HttpCookieCollection<IResponseCookie>();
        }

        /// <summary>
        /// Cookies to send to the server side
        /// </summary>
        public IHttpCookieCollection<IResponseCookie> Cookies { get; set; }
    }
}