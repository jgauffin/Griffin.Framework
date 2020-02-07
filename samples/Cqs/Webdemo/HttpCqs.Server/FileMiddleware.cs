using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;
using HttpCqs.Server.Helpers;

namespace HttpCqs.Server
{
    public class FileMiddleware : HttpMiddleware
    {
        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var request = context.Request;

            //do not do anything, lib will handle it.
            if (request.Uri.AbsolutePath.StartsWith("/cqs"))
            {
                await next();
                return;
            }

            var response = request.CreateResponse();

            var uriWithoutTrailingSlash = request.Uri.AbsolutePath.TrimEnd('/');
            var path = Debugger.IsAttached
                ? Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\public\\",
                    uriWithoutTrailingSlash))
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public\\", uriWithoutTrailingSlash);
            if (path.EndsWith("\\"))
                path = Path.Combine(path, "index.html");

            if (!File.Exists(path))
            {
                response.StatusCode = 404;
                return;
            }

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var extension = Path.GetExtension(path).TrimStart('.');
            response.ContentType = Apache.MimeTypes[extension];
            response.Body = stream;
        }
    }
}