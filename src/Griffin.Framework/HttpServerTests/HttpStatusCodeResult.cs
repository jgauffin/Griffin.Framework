using System.Net;
using Griffin.Net;
using Griffin.Net.Protocols.Http;

namespace HttpServerTests
{
    internal class HttpStatusCodeResult : HttpResult
    {
        public HttpStatusCodeResult(HttpStatusCode httpCode, string reasonPhrase)
        {

        }
    }
}