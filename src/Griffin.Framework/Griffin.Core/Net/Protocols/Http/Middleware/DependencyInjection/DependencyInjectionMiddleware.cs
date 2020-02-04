using System;
using System.Threading.Tasks;
using Griffin.Container;

namespace Griffin.Net.Protocols.Http.Middleware.DependencyInjection
{
    /// <summary>
    /// Generic middleware for dependency injection.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DependencyInjectionMiddleware<TContext> : IMiddleware<TContext> where TContext : IMiddlewareContext
    {
        private readonly IContainer _container;

        public DependencyInjectionMiddleware(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task Process(TContext context, Func<Task> next)
        {
            using (var scope = _container.CreateScope())
            {
                context.ChannelData.Set(scope);
                await next();
            }
        }
    }
}