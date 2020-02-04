using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;

namespace HttpServerTests
{
    public class MyHttpMiddleware : HttpMiddleware
    {
        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Uri.AbsolutePath.StartsWith("/restricted"))
            {
                if (context.User == null)
                {
                    response.StatusCode = 401;
                    return;
                }

                Console.WriteLine("Logged in: " + context.User);
            }

            if (request.Uri.AbsolutePath == "/favicon.ico")
            {
                response.StatusCode = 404;
                response.ContentType = "image/icon";
                response.Body = File.OpenRead("favicon.ico");
                return;
            }

            var msg = Encoding.UTF8.GetBytes("Hello world");
            if (request.Uri.ToString().Contains(".jpg"))
            {
                response.Body = new FileStream(@"C:\users\gaujon01\Pictures\DSC_0231.jpg", FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite);
                response.ContentType = "image/jpeg";
            }
            else
            {
                response.Body = new MemoryStream(msg);
                response.ContentType = "text/plain";
            }
        }
    }
}