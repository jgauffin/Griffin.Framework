using System.Security.Cryptography.X509Certificates;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// A HTTP request where the included body have been parsed
    /// </summary>
    public class HttpRequest : HttpRequestBase, IHttpMessageWithForm
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
        /// <remarks>
        /// <para>
        /// If you override this method you have to copy the PipelineIndexKey header like this:
        /// <code>
        ///  var pipeline = Headers[PipelineIndexKey];
        ///  if (pipeline != null)
        ///  {
        ///     response.Headers[PipelineIndexKey] = pipeline;
        ///  }        
        /// </code>
        /// </para>
        /// </remarks>
        public override IHttpResponse CreateResponse()
        {
            var response = new HttpResponse(200, "OK", HttpVersion);
            var pipeline = Headers[PipelineIndexKey];
            if (pipeline != null)
            {
                response.Headers[PipelineIndexKey] = pipeline;
            }
            return response;
        }
    }
}
