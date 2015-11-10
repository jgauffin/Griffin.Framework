using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Uses an inversion of control container to resolve and execute query handlers.
    /// </summary>
    public class IocQueryBus : IQueryBus
    {
        private readonly IContainer _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocQueryBus" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocQueryBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// A query have been executed.
        /// </summary>
        public event EventHandler<QueryExecutedEventArgs> QueryExecuted;

        /// <summary>
        ///     Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>
        ///     Task which will complete once we've got the result (or something failed, like a query wait timeout).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">query</exception>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            var handler = typeof (IQueryHandler<,>).MakeGenericType(query.GetType(), typeof (TResult));

            using (var scope = _container.CreateScope())
            {
                if (ScopeCreated != null)
                {
                    var createdEventArgs = new ScopeCreatedEventArgs(scope);
                    ScopeCreated(this, createdEventArgs);
                }

                var handlerList = scope.ResolveAll(handler);
                var handlers = handlerList.ToList();

                if (handlers.Count == 0)
                    throw new CqsHandlerMissingException(query.GetType());
                if (handlers.Count != 1)
                    throw new OnlyOneHandlerAllowedException(query.GetType());


                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(query, handlers);
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                var method = handler.GetMethod("ExecuteAsync");
                try
                {
                    var task = (Task) method.Invoke(handlers[0], new object[] {query});
                    await task;
                    var result1= ((dynamic) task).Result;
                    if (QueryExecuted != null)
                        QueryExecuted(this, new QueryExecutedEventArgs(scope, query, handlers[0]));

                  
                    return result1;
                }
                catch (TargetInvocationException exception)
                {
                  
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                    throw new Exception("this will never happen as the line above throws an exception. It's just here to remove a warning");
                }


              
            }
        }


        /// <summary>
        ///     A new IoC container scope have been created (a new scope is created every time a command is about to executed).
        /// </summary>
        public event EventHandler<ScopeCreatedEventArgs> ScopeCreated;
    }
}