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
    ///     Uses reflection to find event subscribers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers must have a default public constructor.
    ///     </para>
    ///     <para>
    ///         This implementation creates a new instance of the handler every time a event is invoked. Handlers that
    ///         implement <see cref="IDisposable" /> will automatically
    ///         be cleaned up when the event has been executed.
    ///     </para>
    /// </remarks>
    public class SimpleEventBus : IEventBus
    {
        private readonly Dictionary<Type, Func<ApplicationEvent, Task>> _handlers =
            new Dictionary<Type, Func<ApplicationEvent, Task>>();

        /// <summary>
        ///     Publish an event.
        /// </summary>
        /// <typeparam name="T">Type of event to execute.</typeparam>
        /// <param name="appEvent">Event to execute</param>
        /// <returns>
        ///     Task which completes once the event has been published.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">event</exception>
        public Task PublishAsync<T>(T appEvent) where T : ApplicationEvent
        {
            Func<ApplicationEvent, Task> handler;
            if (!_handlers.TryGetValue(typeof (T), out handler))
                throw new CqsHandlerMissingException(typeof (T));

            return handler(appEvent);
        }

        /// <summary>
        ///     Register all event handlers that exist in the specified assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan for handlers (implementing <see cref="IApplicationEventSubscriber{TEvent}" />).</param>
        public void Register(Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(IsEventHandler);
            foreach (var handlerType2 in handlers)
            {
                var handlerType = handlerType2;
                var constructor = handlerType.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();
                var handlerMethod = handlerType.GetMethod("HandleAsync");
                var deleg = handlerMethod.ToFastDelegate();
                Func<ApplicationEvent, Task> action = evt =>
                {
                    var handler = factory(handlerType);


                    if (GlobalConfiguration.AuthorizationFilter != null)
                    {
                        var ctx = new AuthorizationFilterContext(evt, new[] { handler });
                        GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                    }

                    var task = (Task) deleg(handler, new object[] {evt});
                    if (handler is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                    return task;
                };

                var intfc = handlerType.GetInterface("IApplicationEventSubscriber`1");
                _handlers[intfc.GetGenericArguments()[0]] = action;
            }
        }

        /// <summary>
        ///     Register a handler.
        /// </summary>
        /// <typeparam name="THandler">Handler</typeparam>
        /// <typeparam name="TEvent">Event that the handler is for</typeparam>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// simpleCmdBus.Register<MySubscriber, UserCreated>();
        /// ]]>
        /// </code>
        /// </example>
        public void Register<THandler, TEvent>()
            where THandler : IApplicationEventSubscriber<TEvent>
            where TEvent : ApplicationEvent
        {
            var handlerType = typeof (THandler);
            var constructor = handlerType.GetConstructor(new Type[0]);
            var factory = constructor.CreateFactory();
            var handlerMethod = handlerType.GetMethod("HandleAsync", new[] { typeof(TEvent) });
            var deleg = handlerMethod.ToFastDelegate();
            Func<ApplicationEvent, Task> action = evt =>
            {
                var handler = factory(handlerType);

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(evt, new[] { handler });
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }


                var task = (Task) deleg(handler, new object[] {evt});
                if (handler is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                return task;
            };

            var intfc = handlerType.GetInterface("IApplicationEventSubscriber`1");
            _handlers[intfc.GetGenericArguments()[0]] = action;
        }

        /// <summary>
        /// Determines whether type implements the event handler interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool IsEventHandler(Type type)
        {
            var intfc = type.GetInterface("IApplicationEventSubscriber`1");
            return intfc != null && !type.IsAbstract && !type.IsInterface;
        }
    }
}