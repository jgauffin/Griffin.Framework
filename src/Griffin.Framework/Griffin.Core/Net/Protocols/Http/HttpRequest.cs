using System.Security.Cryptography.X509Certificates;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// A HTTP request where the included body have been parsed
    /// </summary>
    public class HttpRequest : HttpRequestBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpMethod">Method like <c>POST</c>.</param>
        /// <param name="pathAndQuery">Absolute path and query string (if one exist)</param>
        /// <param name="httpVersion">HTTP version like <c>HTTP/1.1</c></param>
        public HttpRequest(string httpMethod, string pathAndQuery, string httpVersion) : base(httpMethod, pathAndQuery, httpVersion)
        {
            Form = new ParameterCollection();
            Files = new HttpFileCollection();
            Cookies = new HttpCookieCollection<IHttpCookie>();
        }

        /// <summary>
        /// Submitted form items
        /// </summary>
        public IParameterCollection Form { get; set; }

        /// <summary>
        /// Submitted files
        /// </summary>
        public IHttpFileCollection Files { get; set; }

        /// <summary>
        /// Included cookies.
        /// </summary>
        public IHttpCookieCollection<IHttpCookie> Cookies { get; set; }

        /// <summary>
        /// Create a response for this request.
        /// </summary>
        /// <returns>Response</returns>
        public override IHttpResponse CreateResponse()
        {
            return new HttpResponse(200, "OK", HttpVersion);
        }
    }
}
