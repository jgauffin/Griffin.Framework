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
    ///     Uses reflection to find command handlers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The handlers must have a default public constructor.
    ///     </para>
    ///     <para>
    ///         This implementation creates a new instance of the handler every time a command is invoked. Handlers that
    ///         implement <see cref="IDisposable" /> will automatically
    ///         be cleaned up when the command has been executed.
    ///     </para>
    /// </remarks>
    public class SimpleCommandBus : ICommandBus
    {
        private readonly Dictionary<Type, Func<Command, Task>> _commandHandlers =
            new Dictionary<Type, Func<Command, Task>>();

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
        public Task ExecuteAsync<T>(T command) where T : Command
        {
            Func<Command, Task> handler;
            if (!_commandHandlers.TryGetValue(typeof (T), out handler))
                throw new CqsHandlerMissingException(typeof (T));

            return handler(command);
        }

        /// <summary>
        ///     Register all command handlers that exist in the specified assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan for command handlers (implementing <see cref="ICommandHandler{TCommand}" />).</param>
        public void Register(Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(IsCommandHandler);
            foreach (var handlerType2 in handlers)
            {
                var handlerType = handlerType2;
                var constructor = handlerType.GetConstructor(new Type[0]);
                var factory = constructor.CreateFactory();
                var handlerMethod = handlerType.GetMethod("ExecuteAsync");
                var deleg = handlerMethod.ToFastDelegate();
                Func<Command, Task> action = cmd =>
                {
                    var handler = factory(handlerType);

                    if (GlobalConfiguration.AuthorizationFilter != null)
                    {
                        var ctx = new AuthorizationFilterContext(cmd, new[]{handler});
                        GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                    }

                    var task = (Task) deleg(handler, new object[] {cmd});
                    if (handler is IDisposable)
                        task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                    return task;
                };

                var intfc = handlerType.GetInterface("ICommandHandler`1");
                _commandHandlers[intfc.GetGenericArguments()[0]] = action;
            }
        }

        /// <summary>
        ///     Register a command handler
        /// </summary>
        /// <typeparam name="THandler">Handler</typeparam>
        /// <typeparam name="TCommand">Command that the handler is for</typeparam>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// simpleCmdBus.Register<MyHandler, MyCommand>();
        /// ]]>
        /// </code>
        /// </example>
        public void Register<THandler, TCommand>()
            where THandler : ICommandHandler<TCommand>
            where TCommand : Command
        {
            var handlerType = typeof (THandler);
            var constructor = handlerType.GetConstructor(new Type[0]);
            var factory = constructor.CreateFactory();
            var handlerMethod = handlerType.GetMethod("ExecuteAsync", new[] {typeof (TCommand)});
            var deleg = handlerMethod.ToFastDelegate();
            Func<Command, Task> action = cmd =>
            {
                var handler = factory(handlerType);

                if (GlobalConfiguration.AuthorizationFilter != null)
                {
                    var ctx = new AuthorizationFilterContext(cmd, new[] { handler });
                    GlobalConfiguration.AuthorizationFilter.Authorize(ctx);
                }

                var task = (Task) deleg(handler, new object[] {cmd});
                if (handler is IDisposable)
                    task.ContinueWith(t2 => ((IDisposable) handler).Dispose());
                return task;
            };

            var intfc = handlerType.GetInterface("ICommandHandler`1");
            _commandHandlers[intfc.GetGenericArguments()[0]] = action;
        }

        public static bool IsCommandHandler(Type type)
        {
            var intfc = type.GetInterface("ICommandHandler`1");
            return intfc != null && !type.IsAbstract && !type.IsInterface;
        }
    }
}