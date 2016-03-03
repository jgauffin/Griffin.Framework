using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Uses your inversion of control container to identify all event handlers.
    /// </summary>
    /// <remarks>
    ///     <para>Creates one child scope per event handler each time and event is being published.</para>
    /// </remarks>
    public class SeparateScopesIocEventBus : IEventBus
    {
        private readonly IContainer _container;
        private readonly IEventHandlerRegistry _registry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocEventBus" /> class.
        /// </summary>
        /// <param name="container">Used to resolve <c><![CDATA[IApplicationEventSubscriber<TApplicationEvent>]]></c>.</param>
        /// <param name="registry">
        ///     Used to be able to identify all concrete events handler to be able to resolve them in seperate scopes. This also
        ///     means that each
        ///     handler have to be registered as itself in the container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public SeparateScopesIocEventBus(IContainer container, IEventHandlerRegistry registry)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            _container = container;
            _registry = registry;
        }

        /// <summary>
        ///     Publish a new application event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event to publish.</typeparam>
        /// <param name="e">Event to publish, must be serializable.</param>
        /// <returns>
        ///     Task triggered once the event has been delivered.
        /// </returns>
        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e)
            where TApplicationEvent : ApplicationEvent
        {
            var handlers = _registry.Lookup(typeof(TApplicationEvent)).ToList();
            if (GlobalConfiguration.AuthorizationFilter != null)
            {
                var ctx = new AuthorizationFilterContext(e, handlers);
                GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
            }

            var eventInfo = new List<EventHandlerInfo>();
            var tasks = handlers
                .Select(handler => InvokeHandlerAsync(handler, e, eventInfo))
                .ToList();

            Task task = null;
            try
            {
                task = Task.WhenAll(tasks);
                await task;
                EventPublished(this, new EventPublishedEventArgs(null, e, true, eventInfo));
            }
            catch
            {
                EventPublished(this, new EventPublishedEventArgs(null, e, false, eventInfo));
                var failures = eventInfo.Where(x => x.Failure != null).Select(x => x.Failure).ToList();
                HandlerFailed(this, new EventHandlerFailedEventArgs(e, failures, eventInfo.Count));
                throw task.Exception;
            }
        }

        private async Task InvokeHandlerAsync<TEventType>(Type eventHandlerType, TEventType e,
            ICollection<EventHandlerInfo> eventInfo) where TEventType : ApplicationEvent
        {
            using (var scope = _container.CreateScope())
            {
                ScopeCreatedEventArgs createdEventArgs = null;
                if (ScopeCreated != null)
                {
                    createdEventArgs = new ScopeCreatedEventArgs(scope);
                    ScopeCreated(this, createdEventArgs);
                }


                var subscriber = scope.Resolve(eventHandlerType);
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    await ((IApplicationEventSubscriber<TEventType>)subscriber).HandleAsync(e);
                    eventInfo.Add(new EventHandlerInfo(subscriber.GetType(), sw.ElapsedMilliseconds));
                    if (ScopeClosing != null)
                        ScopeClosing(this, new ScopeClosingEventArgs(scope) { HandlersWasSuccessful = true });
                }
                catch (Exception ex)
                {
                    eventInfo.Add(new EventHandlerInfo(subscriber.GetType(), sw.ElapsedMilliseconds)
                    {
                        Failure = new HandlerFailure(subscriber, ex)
                    });
                    if (ScopeClosing != null)
                        ScopeClosing(this, new ScopeClosingEventArgs(scope) { HandlersWasSuccessful = false });
                    throw;
                }


            }
        }

        /// <summary>
        ///     Event have been executed and the scope will be disposed after this event has been triggered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can use this event to save a Unit Of Work or similar.
        ///     </para>
        ///     <para>
        ///         Are also invoked if on or more handlers failed.
        ///     </para>
        /// </remarks>
        public event EventHandler<EventPublishedEventArgs> EventPublished = delegate { };

        /// <summary>
        ///     A specific handler failed to consume the application event.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We will not try to invoke the event again as one or more handlers may have consumed the event successfully.
        ///     </para>
        /// </remarks>
        public event EventHandler<EventHandlerFailedEventArgs> HandlerFailed = delegate { };

        /// <summary>
        ///     A new IoC container scope have been created (a new scope is created every time a command is about to executed).
        /// </summary>
        public event EventHandler<ScopeCreatedEventArgs> ScopeCreated;

        /// <summary>
        ///     One of the created scopes is about to close.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Look at the <see cref="ScopeClosingEventArgs.HandlersWasSuccessful" /> property to determine if the handler(s)
        ///         executed successfully.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// eventBus.ScopeClosing += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<ScopeClosingEventArgs> ScopeClosing;
    }
}