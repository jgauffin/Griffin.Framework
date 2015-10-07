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

                var eventInfo = new List<EventHandlerInfo>();
                var tasks = implementations.Select(x => PublishEvent(x,e, eventInfo));
                Task task = null;
                try
                {
                    task = Task.WhenAll(tasks);
                    await task;
                    EventPublished(this, new EventPublishedEventArgs(scope, e, true, eventInfo));
                }
                catch
                {
                    EventPublished(this, new EventPublishedEventArgs(scope, e, false, eventInfo));
                    var failures = eventInfo.Where(x => x.Failure != null).Select(x => x.Failure).ToList();
                    HandlerFailed(this, new EventHandlerFailedEventArgs(e, failures, eventInfo.Count));
                    throw task.Exception;
                }
            }
        }

        /// <summary>
        /// Purpose of this method is to be able to meassure invocation times.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber"></param>
        /// <param name="e"></param>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        private async Task PublishEvent<T>(IApplicationEventSubscriber<T> subscriber, T e, ICollection<EventHandlerInfo> eventInfo) where T : ApplicationEvent
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                await subscriber.HandleAsync(e);
                eventInfo.Add(new EventHandlerInfo(subscriber.GetType(), sw.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                eventInfo.Add(new EventHandlerInfo(subscriber.GetType(), sw.ElapsedMilliseconds)
                {
                    Failure = new HandlerFailure(subscriber, ex)
                });
                throw;
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