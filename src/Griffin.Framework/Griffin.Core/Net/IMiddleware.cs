using System;
using System.Threading.Tasks;

namespace Griffin.Net
{
    public interface IMiddleware<in TContext> where TContext : IMiddlewareContext
    {
        Task Process(TContext context, Func<Task> next);
    }
}