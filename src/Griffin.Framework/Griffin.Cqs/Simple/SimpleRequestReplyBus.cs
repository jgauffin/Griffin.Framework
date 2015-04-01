using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.Simple
{
    /// <summary>
    ///     Uses reflection to find request/reply handlers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers must have a default public constructor.
    ///     </para>
    ///     <para>
    ///         This implementation creates a new instance of the handler every time a request is invoked. Handlers that
    ///         implement <see cref="IDisposable" /> will automatically
    ///         be cleaned up when the query has been executed.
    ///     </para>
    /// </remarks>
    public class SimpleRequestReplyBus : IRequestReplyBus
    {
        private readonly Dictionary<Type, Func<IRequest, Task>> _handlers =
            new Dictionary<Type, Func<IRequest, Task>>();

        /// <summary>
        ///     Execute request and return a reply
        /// </summary>
        /// <typeparam name="TReply">reply that the request will return.</typeparam>
        /// <param name="request">Request to execute</param>
        /// <returns>
        ///     Task which completes once the request has been executed (and a reply has been fetched).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">query</exception>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            Func<IRequest, Task> handler;
            if (!_handlers.TryGetValue(request.GetType(), out handler))
                throw new CqsHandlerMissingException(request.GetType());

            var task = (Task<TReply>) handler(request);
            await task;
            return task.Result;
        }

        /// <summary>
        ///     Register all event handlers that exist in the specified assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan for handlers (implementing <see cref="IRequestHandler{TRequest,TReply}" />).</param>
        public void Register(Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(IsRequestHandler);
            foreach (var handlerType2 in handlers)
            {
                var handlerType = handlerType2;

                var constructor = handlerType.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();
                var handlerMethod = handlerType.GetMethod("ExecuteAsync");
                var deleg = handlerMethod.ToFastDelegate();
                Func<IRequest, Task> action = request =>
                {
                    var handler = factory(handlerType);

                    if (GlobalConfiguration.AuthorizationFilter != null)
                    {
                        var ctx = new AuthorizationFilterContext(request, new[] { handler });
                        GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                    }

                    var task = (Task) deleg(handler, new object[] {request});
                    if (handler is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                    return task;
                };

                var intfc = handlerType.GetInterface("IRequestHandler`2");
                _handlers[intfc.GetGenericArguments()[0]] = action;
            }
        }

        /// <summary>
        ///     Register a handler.
        /// </summary>
        /// <typeparam name="THandler">Handler</typeparam>
        /// <typeparam name="TQuery">Request that the handler is for</typeparam>
        /// <typeparam name="TResult">Reply that the request handler will return.</typeparam>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// simpleRequestBus.Register<LoginHandler, Login, LoginResult>();
        /// ]]>
        /// </code>
        /// </example>
        public void Register<THandler, TQuery, TResult>()
            where THandler : IQueryHandler<TQuery, TResult>
            where TQuery : Query<TResult>
        {
            var handlerType = typeof (THandler);
            var constructor = handlerType.GetConstructor(new Type[0]);
            var factory = constructor.CreateFactory();
            var handlerMethod = handlerType.GetMethod("ExecuteAsync", new[] {typeof (TQuery)});
            var deleg = handlerMethod.ToFastDelegate();
            Func<IRequest, Task> action = request =>
            {
                var handler = factory(handlerType);

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(request, new[] { handler });
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                var task = (Task) deleg(handler, new object[] {request});
                if (handler is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                return task;
            };

            var intfc = handlerType.GetInterface("IRequestHandler`2");
            _handlers[intfc.GetGenericArguments()[0]] = action;
        }

        /// <summary>
        /// Determines whether the specified type implements the request handler interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsRequestHandler(Type type)
        {
            var intfc = type.GetInterface("IRequestHandler`2");
            return intfc != null && !type.IsAbstract && !type.IsInterface;
        }
    }
}