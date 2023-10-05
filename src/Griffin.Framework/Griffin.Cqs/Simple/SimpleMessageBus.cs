using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.Simple;

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
public class SimpleMessageBus : IMessageBus
{
    private readonly Dictionary<Type, Func<object, Task>> _handlers = new();

    public Task SendAsync(ClaimsPrincipal principal, object message)
    {
        return SendAsync(message);
    }

    public Task SendAsync(ClaimsPrincipal principal, Message message)
    {
        return SendAsync(message.Body);
    }

    public Task SendAsync(Message message)
    {
        return SendAsync(message.Body);
    }

    public async Task SendAsync(object message)
    {
        if (!_handlers.TryGetValue(message.GetType(), out var handler))
            throw new CqsHandlerMissingException(message.GetType());

        await handler(message);
    }

    /// <summary>
    ///     Determines whether the specified type implements the request handler interface.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static bool IsRequestHandler(Type type)
    {
        var intfc = type.GetInterface("IRequestHandler`2");
        return intfc != null && !type.IsAbstract && !type.IsInterface;
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
            Func<object, Task> action = request =>
            {
                var handler = factory(handlerType);

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(request, new[] { handler });
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                var task = (Task)deleg(handler, new[] { request });
                if (handler is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable)handler).Dispose());
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
    /// <typeparam name="TMessage">Message that the handler is for</typeparam>
    /// <example>
    ///     <code>
    /// <![CDATA[
    /// simpleRequestBus.Register<LoginHandler, Login, LoginResult>();
    /// ]]>
    /// </code>
    /// </example>
    public void Register<THandler, TMessage>()
        where THandler : IMessageHandler<TMessage>
    {
        var handlerType = typeof(THandler);
        var constructor = handlerType.GetConstructor(new Type[0]);
        var factory = constructor.CreateFactory();
        var handlerMethod = handlerType.GetMethod("HandleAsync", new[] { typeof(TMessage) });
        var deleg = handlerMethod.ToFastDelegate();
        Func<object, Task> action = request =>
        {
            var handler = factory(handlerType);

            if (GlobalConfiguration.AuthorizationFilter != null)
            {
                var ctx = new AuthorizationFilterContext(request, new[] { handler });
                GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
            }

            var task = (Task)deleg(handler, new[] { request });
            if (handler is IDisposable)
                task.ContinueWith(t2 => ((IDisposable)handler).Dispose());
            return task;
        };

        var intfc = handlerType.GetInterface("IRequestHandler`2");
        _handlers[intfc.GetGenericArguments()[0]] = action;
    }
}