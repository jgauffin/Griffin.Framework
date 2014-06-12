using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Simple
{
    /// <summary>
    ///     Uses reflection to find query handlers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers must have a default public constructor.
    ///     </para>
    ///     <para>
    ///         This implementation creates a new instance of the handler every time a query is invoked. Handlers that
    ///         implement <see cref="IDisposable" /> will automatically
    ///         be cleaned up when the query has been executed.
    ///     </para>
    /// </remarks>
    public class SimpleQueryBus : IQueryBus
    {
        private readonly Dictionary<Type, Func<IQuery, Task>> _handlers =
            new Dictionary<Type, Func<IQuery, Task>>();

        /// <summary>
        ///     Request that a query should be executed.
        /// </summary>
        /// <typeparam name="TResult">Result that the query will return.</typeparam>
        /// <param name="query">Query to execute</param>
        /// <returns>
        ///     Task which completes once the query has been executed (and a response have been fetched).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">query</exception>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            Func<IQuery, Task> handler;
            if (!_handlers.TryGetValue(query.GetType(), out handler))
                throw new CqsHandlerMissingException(query.GetType());

            var task = (Task<TResult>) handler(query);
            await task;
            return task.Result;
        }

        /// <summary>
        ///     Register all event handlers that exist in the specified assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan for handlers (implementing <see cref="IQueryHandler{TQuery,TResult}" />).</param>
        public void Register(Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(IsHandler);
            foreach (var handler in handlers)
            {
                var constructor = handler.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();

                var intfc = handler.GetInterface("IQueryHandler`2");

                var handlerMethod = handler.GetMethod("ExecuteAsync");//.MakeGenericMethod(intfc.GetGenericArguments()[1]);
                var deleg = handlerMethod.ToFastDelegate();
                Func<IQuery, Task> action = cmd =>
                {
                    var t = factory(handler);
                    var task = (Task) deleg(t, new object[] {cmd});
                    if (t is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                    return task;
                };

                
                _handlers[intfc.GetGenericArguments()[0]] = action;
            }
        }

        /// <summary>
        ///     Register a handler.
        /// </summary>
        /// <typeparam name="THandler">Handler</typeparam>
        /// <typeparam name="TQuery">Query that the handler is for</typeparam>
        /// <typeparam name="TResult">Response that the query returns</typeparam>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// simpleCmdBus.Register<FindUsersHandler, FindUsers, User[]>();
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
            Func<IQuery, Task> action = cmd =>
            {
                var t = factory(handler);
                var task = (Task) deleg(t, new object[] {cmd});
                if (t is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                return task;
            };

            var intfc = handler.GetInterface("IQueryHandler`2");
            _handlers[intfc.GetGenericArguments()[0]] = action;
        }

        private static bool IsHandler(Type arg)
        {
            var intfc = arg.GetInterface("IQueryHandler`2");
            return intfc != null && !arg.IsAbstract && !arg.IsInterface;
        }
    }
}