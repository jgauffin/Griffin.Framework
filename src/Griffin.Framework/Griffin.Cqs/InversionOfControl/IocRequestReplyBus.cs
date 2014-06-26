using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Execute requests in child scopes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Will create a new container child scope for every request that is executed. If you want to save a unit of work
    ///         or similar just hookup on the <see cref="RequestInvoked" /> event which is fired
    ///         upon successful execution.
    ///     </para>
    /// </remarks>
    public class IocRequestReplyBus : IRequestReplyBus
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocRequestReplyBus"/> class.
        /// </summary>
        /// <param name="container">Used to resolve <c><![CDATA[IRequestHandler<,>]]></c>.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocRequestReplyBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        ///     Invoke a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">Type of reply that we should get for the request.</typeparam>
        /// <param name="request">Request that we want a reply for.</param>
        /// <returns>
        ///     Task which will complete once we've got the result (or something failed, like a request wait timeout).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">query</exception>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            var handler = typeof (IRequestHandler<,>).MakeGenericType(request.GetType(), typeof (TReply));

            using (var scope = _container.CreateScope())
            {
                var allHandlersAsObjects = scope.ResolveAll(handler);
                var handlers = allHandlersAsObjects.ToArray();

                if (handlers.Length == 0)
                    throw new CqsHandlerMissingException(request.GetType());
                if (handlers.Length != 1)
                    throw new OnlyOneHandlerAllowedException(request.GetType());


                try
                {
                    var method = handler.GetMethod("ExecuteAsync");
                    var task = (Task) method.Invoke(handlers[0], new object[] {request});
                    await task;
                    var result = ((dynamic) task).Result;
                    RequestInvoked(this, new RequestInvokedEventArgs(scope, request));
                    return result;
                }
                catch (TargetInvocationException exception)
                {
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                    throw new Exception(
                        "this will never happen as the line above throws an exception. It's just here to remove a warning");
                }
            }
        }

        /// <summary>
        ///     Request have been successfully executed and the scope will be disposed after this event has been triggered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can use the event to save a Unit Of Work or similar.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// requestReplyBus.RequestInvoked += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<RequestInvokedEventArgs> RequestInvoked = delegate { };
    }
}