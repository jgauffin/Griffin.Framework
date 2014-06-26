using System;
using DotNetCqs;
using Griffin.Cqs.InversionOfControl;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Used by <see cref="QueuedIocCommandBus.HandlerFailed" />.
    /// </summary>
    public class CommandHandlerFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerFailedEventArgs"/> class.
        /// </summary>
        /// <param name="command">command that was invoked.</param>
        /// <param name="handler">handler that failed.</param>
        /// <param name="exception">exception that the handler threw.</param>
        /// <exception cref="System.ArgumentNullException">
        /// command
        /// or
        /// handler
        /// or
        /// exception
        /// </exception>
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