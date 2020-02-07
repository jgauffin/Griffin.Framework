using System.Net;
using Griffin.Net.Protocols.Http;

namespace HttpServerTests.Results
{
    internal class HttpStatusCodeResult : HttpResult
    {
        public HttpStatusCodeResult(HttpStatusCode httpCode, string reasonPhrase)
        {

        }
    }
}