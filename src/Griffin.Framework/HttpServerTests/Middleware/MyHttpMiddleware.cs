using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;

namespace HttpServerTests.Middleware
{
    public class MyHttpMiddleware : HttpMiddleware
    {
        private readonly string _wwwDirectory;

        public MyHttpMiddleware()
        {
            _wwwDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var pos = _wwwDirectory.IndexOf("bin\\");
            if (pos != -1 && Debugger.IsAttached)
                _wwwDirectory = _wwwDirectory.Substring(0, pos);

            _wwwDirectory = Path.Combine(_wwwDirectory, "wwwroot");
        }

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

            var absoluteUri = context.Request.Uri.AbsolutePath;
            absoluteUri = absoluteUri == "/" ? "index.html" : absoluteUri.TrimStart('/');

            var fullPath = Path.Combine(_wwwDirectory, absoluteUri.Replace("/", "\\"));
            if (!File.Exists(fullPath))
            {
                response.StatusCode = 404;
                return;
            }


            var extensionPos = fullPath.LastIndexOf('.');
            var extension = extensionPos == -1 ? "" : fullPath.Substring(extensionPos + 1);
            string mimeType;
            switch (extension)
            {
                case "png":
                    mimeType = "image/png";
                    break;
                case "webmanifest":
                    mimeType = "application/manifest+json";
                    break;
                case "html":
                    mimeType = "text/html";
                    break;
                case "ico":
                    mimeType = "image/x-icon";
                    break;

                default:
                    mimeType = "application/octet-stream";
                    break;
            }

            response.AddHeader("Content-Type", mimeType);
            response.Body = File.OpenRead(fullPath);
        }
    }
}