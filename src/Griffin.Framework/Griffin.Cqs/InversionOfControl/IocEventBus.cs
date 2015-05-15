using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Uses your favorite inversion of control container to publish events
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers will be invoked asynchronously, but the publish method will not return before all subscribers have
    ///         finished their processing.
    ///     </para>
    /// </remarks>
    public class IocEventBus : IEventBus
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocEventBus"/> class.
        /// </summary>
        /// <param name="container">Used to resolve <c><![CDATA[IApplicationEventSubscriber<TApplicationEvent>]]></c>.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocEventBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
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
            using (var scope = _container.CreateScope())
            {
                var implementations = scope
                    .ResolveAll<IApplicationEventSubscriber<TApplicationEvent>>()
                    .ToList();

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(e, implementations);
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                var tasks = implementations.Select(x => x.HandleAsync(e));
                Task task = null;
                try
                {
                    task = Task.WhenAll(tasks);
                    await task;
                    EventPublished(this, new EventPublishedEventArgs(scope, e, true));
                }
                catch
                {
                    EventPublished(this, new EventPublishedEventArgs(scope, e, false));
                    throw task.Exception;
                }
            }
        }

      
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
        ///     Event have been executed and the scope will be disposed after this event has been triggered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can use this event to save a Unit Of Work or similar.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// eventBus.EventPublished += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<EventPublishedEventArgs> EventPublished = delegate { };
    }
}