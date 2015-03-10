using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Executes commands in a new scope.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Will create a new container child scope for every command that is executed. If you want to save a unit of work
    ///         or similar just hookup on the <see cref="CommandInvoked" /> event which is fired
    ///         upon successful execution.
    ///     </para>
    /// </remarks>
    public class IocCommandBus : ICommandBus
    {
        private readonly IContainer _container;
        private IAuthorizationFilter _authorizationFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocCommandBus"/> class.
        /// </summary>
        /// <param name="container">Used to lookup <see cref="ICommandHandler{TCommand}"/>.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocCommandBus(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }


        /// <summary>
        ///     Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>
        ///     Task which completes once the command has been delivered (and NOT when it has been executed).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">command</exception>
        /// <remarks>
        ///     <para>
        ///         The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed
        ///         just because this method returns. That just means
        ///         that the command have been successfully delivered (to a queue or another process etc) for execution.
        ///     </para>
        /// </remarks>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            using (var scope = _container.CreateScope())
            {
                var handlers = scope.ResolveAll<ICommandHandler<T>>().ToList();
                if (handlers.Count == 0)
                    throw new CqsHandlerMissingException(typeof(T));
                if (handlers.Count > 1)
                    throw new OnlyOneHandlerAllowedException(typeof(T));

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(command, handlers);
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                await handlers[0].ExecuteAsync(command);
                CommandInvoked(this, new CommandInvokedEventArgs(scope, command));
            }
        }

        /// <summary>
        ///     Command have been successfully executed and the scope will be disposed after this event has been triggered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can use the event to save a Unit Of Work or similar.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// commandBus.CommandInvoked += (source,e) => e.Scope.ResolveAll<IUnitOfWork>().SaveChanges();
        /// ]]>
        /// </code>
        /// </example>
        public event EventHandler<CommandInvokedEventArgs> CommandInvoked = delegate { };
    }
}