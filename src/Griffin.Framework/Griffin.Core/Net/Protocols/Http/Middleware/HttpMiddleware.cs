using System;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.Middleware
{
    public abstract class HttpMiddleware : IMiddleware<HttpContext>
    {
        public abstract Task Process(HttpContext context, Func<Task> next);
    }
}