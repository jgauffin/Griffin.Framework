using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.Results
{
    public class StandardResultInvokers : 
        IHttpResultInvoker<ContentResult>, 
        IHttpResultInvoker<RedirectResult>,
        IHttpResultInvoker<HttpResponseResult>
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        Task IHttpResultInvoker<ContentResult>.InvokeAsync(HttpResultInvokerContext context, ContentResult result)
        {
            Stream stream;
            if (result.StringContent != null)
            {
                var buf = Encoding.GetBytes(result.StringContent);
                stream = new MemoryStream(buf, 0, buf.Length, true, true);
            }
            else
            {
                stream = result.BinaryContent;
            }

            context.HttpResponse.ContentType = result.ContentType;
            context.HttpResponse.Body = stream;
            return Task.FromResult<object>(null);
        }

        Task IHttpResultInvoker<RedirectResult>.InvokeAsync(HttpResultInvokerContext context, RedirectResult result)
        {
            var schemePos = result.Location.IndexOf("://", StringComparison.Ordinal);
            var requestUri = context.HttpContext.Request.Uri;
            Uri uri;
            if (result.Location.StartsWith("/"))
            {
                var port = "";
                if (requestUri.Port != 0 && requestUri.Port != 80)
                    port = $":{requestUri.Port}";

                uri = new Uri($"{requestUri.Scheme}://{requestUri.Host}{port}{result.Location}");
            }
            else if (schemePos == -1)
            {
                var uri2=new Uri(result.Location, UriKind.Relative);
                uri = new Uri(requestUri, uri2);
            }
            else
            {
                uri = new Uri(result.Location);
            }

            context.HttpResponse.Headers["Location"] = uri.ToString();
            context.HttpResponse.StatusCode = 301;
            return Task.FromResult<object>(null);
        }

        public Task InvokeAsync(HttpResultInvokerContext context, HttpResponseResult result)
        {
            context.HttpResponse = result.Response;
            return Task.FromResult<object>(null);

        }
    }
}