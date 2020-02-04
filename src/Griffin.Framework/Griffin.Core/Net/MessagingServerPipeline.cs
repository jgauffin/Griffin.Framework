using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Results;

namespace Griffin.Net
{
    public class MessagingServerPipeline<TContext> where TContext : IMiddlewareContext
    {
        private readonly List<IMiddleware<TContext>> _middlewares = new List<IMiddleware<TContext>>();
        public int Count => _middlewares.Count;

        public async Task Execute(TContext context)
        {
            await ExecuteMiddleware(0, context);
        }

        public void ProcessError(TContext context, ServerErrorResult result)
        {
            result.Shutdown = true;
        }

        public void ProcessResult(HttpContext httpContext, HttpResult result)
        {
            throw new NotImplementedException();
        }

        public void Register(IMiddleware<TContext> middleware)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            _middlewares.Add(middleware);
        }

        private async Task ExecuteMiddleware(int index, TContext context)
        {
#if NET452
            Task completedTask = Task.FromResult<object>(null);
#else
            Task completedTask = Task.CompletedTask;
#endif

            if (index == _middlewares.Count - 1)
                await _middlewares[index].Process(context, () => completedTask);
            else
                await _middlewares[index].Process(context, async () => await ExecuteMiddleware(index + 1, context));
        }
    }
}