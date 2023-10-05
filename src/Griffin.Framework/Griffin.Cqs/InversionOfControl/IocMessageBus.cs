using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs;
using DotNetCqs.DependencyInjection;
using Griffin.Container;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Uses your favorite inversion of control container to publish events.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers will be invoked asynchronously, but the publish method will not return before all subscribers have
    ///         finished their processing.
    ///     </para>
    /// <para>This implementation uses a single IoC scope for all handlers. If you want to have a separate scope per handler use <see cref="SeparateScopesIocEventBus"/> instead.</para>
    /// </remarks>
    public class IocMessageBus : IMessageBus
    {
        private readonly IContainer _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocMessageBus" /> class.
        /// </summary>
        /// <param name="container">Used to resolve <c><![CDATA[IApplicationEventSubscriber<TApplicationEvent>]]></c>.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocMessageBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

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
        /// eventBus.MessageHandled += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<MessagePublishedEventArgs> MessageHandled = delegate { };

        /// <summary>
        ///     A specific handler failed to consume the application event.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We will not try to invoke the event again as one or more handlers may have consumed the event successfully.
        ///     </para>
        /// </remarks>
        public event EventHandler<MessageHandlerFailedEventArgs> HandlerFailed = delegate { };

        /// <summary>
        ///     A new IoC container scope have been created (a new scope is created every time a command is about to executed).
        /// </summary>
        public event EventHandler<ScopeCreatedEventArgs> ScopeCreated;


        public Task SendAsync(ClaimsPrincipal principal, object message)
        {
            return SendAsync(message);
        }

        public Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            return SendAsync(principal, message.Body);
        }

        public Task SendAsync(Message message)
        {
            return SendAsync(message.Body);

        }

        public async Task SendAsync(object message)
        {
            using var scope = _container.CreateScope();
            if (ScopeCreated != null)
            {
                var createdEventArgs = new ScopeCreatedEventArgs(scope);
                ScopeCreated(this, createdEventArgs);
            }

            var genericHandlerType = typeof(IMessageHandler<>).MakeGenericType(message.GetType());
            var implementations = scope
                .ResolveAll(genericHandlerType)
                .ToList();

            if (GlobalConfiguration.AuthorizationFilter != null)
            {
                var ctx = new AuthorizationFilterContext(message, implementations);
                GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
            }

            var method = genericHandlerType.GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public, null,
                new Type[] { typeof(IMessageContext), message.GetType() }, null)!;

            var eventInfo = new List<MessageHandlerInfo>();
            var tasks = implementations.Select(x => HandleMessage(method, x, message, eventInfo));
            Task task = null;
            try
            {
                task = Task.WhenAll(tasks);
                await task;
                MessageHandled(this, new MessagePublishedEventArgs(scope, message, true, eventInfo));
            }
            catch
            {
                MessageHandled(this, new MessagePublishedEventArgs(scope, message, false, eventInfo));
                var failures = eventInfo.Where(x => x.Failure != null).Select(x => x.Failure).ToList();
                HandlerFailed(this, new MessageHandlerFailedEventArgs(message, failures, eventInfo.Count));

                throw task.Exception;
            }
        }

        private Task HandleMessage(MethodInfo methodInfo, object instance, object message,
            List<MessageHandlerInfo> infos)
        {
            try
            {
                IMessageContext ctx = new MessageInvocationContext();
                methodInfo.Invoke(instance)
            }
        }
    }
}