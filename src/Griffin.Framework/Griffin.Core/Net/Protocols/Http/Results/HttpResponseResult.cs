namespace Griffin.Net.Protocols.Http.Results
{
    public class HttpResponseResult : HttpResult
    {
        public HttpResponseResult(HttpResponse response)
        {
            Response = response;
        }

        public HttpResponse Response { get; }
    }
}