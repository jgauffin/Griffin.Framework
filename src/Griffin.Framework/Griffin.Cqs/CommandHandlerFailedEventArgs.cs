using System;
using DotNetCqs;
using Griffin.Cqs.InversionOfControl;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Used by <see cref="ContainerCommandBus.HandlerFailed" />.
    /// </summary>
    public class CommandHandlerFailedEventArgs : EventArgs
    {
        public CommandHandlerFailedEventArgs(Command command, object handler, Exception exception)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (handler == null) throw new ArgumentNullException("handler");
            if (exception == null) throw new ArgumentNullException("exception");
            Command = command;
            Handler = handler;
            Exception = exception;
        }

        /// <summary>
        ///     command that we could not execute
        /// </summary>
        public Command Command { get; private set; }

        /// <summary>
        ///     Handler that failed to execute the command
        /// </summary>
        public object Handler { get; private set; }

        /// <summary>
        ///     Why the execution failed.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}