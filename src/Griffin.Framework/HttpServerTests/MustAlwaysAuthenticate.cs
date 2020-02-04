using System;
using System.Threading.Tasks;
using Griffin.Net;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;

namespace HttpServerTests
{
    internal class MustAlwaysAuthenticate : HttpMiddleware
    {
        public override Task Process(HttpContext context, Func<Task> next)
        {
            if (!context.Request.Headers.Contains("Authenticate"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            return next();
        }
    }
}