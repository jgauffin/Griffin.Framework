using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;

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
            var handlers = assembly.GetTypes().Where(IsHandler);
            foreach (var handler in handlers)
            {
                var constructor = handler.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();
                var handlerMethod = handler.GetMethod("PublishAsync");
                var deleg = handlerMethod.ToFastDelegate();
                Func<ApplicationEvent, Task> action = cmd =>
                {
                    var t = factory(handler);
                    var task = (Task) deleg(t, new object[] {cmd});
                    if (t is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                    return task;
                };

                var intfc = handler.GetInterface("IApplicationEventSubscriber`1");
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
            var handler = typeof (THandler);
            var constructor = handler.GetConstructor(new Type[0]);
            var factory = constructor.CreateFactory();
            var handlerMethod = handler.GetMethod("PublishAsync", new[] { typeof(TEvent) });
            var deleg = handlerMethod.ToFastDelegate();
            Func<ApplicationEvent, Task> action = cmd =>
            {
                var t = factory(handler);
                var task = (Task) deleg(t, new object[] {cmd});
                if (t is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) t).Dispose());
                return task;
            };

            var intfc = handler.GetInterface("IApplicationEventSubscriber`1");
            _handlers[intfc.GetGenericArguments()[0]] = action;
        }

        private static bool IsHandler(Type arg)
        {
            var intfc = arg.GetInterface("IApplicationEventSubscriber`1");
            return intfc != null && !arg.IsAbstract && !arg.IsInterface;
        }
    }
}