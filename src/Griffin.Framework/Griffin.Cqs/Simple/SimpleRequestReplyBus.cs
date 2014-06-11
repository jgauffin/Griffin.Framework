using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;

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
        ///     Task which completes once the request has been executed (and a reply have been fetched).
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
            var handlers = assembly.GetTypes().Where(IsHandler);
            foreach (var handler in handlers)
            {
                var constructor = handler.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();
                var handlerMethod = handler.GetMethod("ExecuteAsync");
                var deleg = handlerMethod.ToFastDelegate();
                Func<IRequest, Task> action = cmd =>
                {
                    var t = factory(handler);
                    var task = (Task) deleg(t, new object[] {cmd});
                    if (t is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                    return task;
                };

                var intfc = handler.GetInterface("IRequestHandler`1");
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
            var handler = typeof (THandler);
            var constructor = handler.GetConstructor(new Type[0]);
            var factory = constructor.CreateFactory();
            var handlerMethod = handler.GetMethod("ExecuteAsync", new[] {typeof (TQuery)});
            var deleg = handlerMethod.ToFastDelegate();
            Func<IRequest, Task> action = cmd =>
            {
                var t = factory(handler);
                var task = (Task) deleg(t, new object[] {cmd});
                if (t is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                return task;
            };

            var intfc = handler.GetInterface("IRequestHandler`1");
            _handlers[intfc.GetGenericArguments()[0]] = action;
        }

        private static bool IsHandler(Type arg)
        {
            var intfc = arg.GetInterface("IRequestHandler`1");
            return intfc != null && !arg.IsAbstract && !arg.IsInterface;
        }
    }
}