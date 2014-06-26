using System;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// A command have been successfully invoked
    /// </summary>
    public class CommandInvokedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInvokedEventArgs"/> class.
        /// </summary>
        /// <param name="scope">Scope that the command was invoked in (scope is still open).</param>
        /// <param name="command">Command that was executed.</param>
        /// <exception cref="System.ArgumentNullException">
        /// scope
        /// or
        /// command
        /// </exception>
        public CommandInvokedEventArgs(IContainerScope scope, Command command)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (command == null) throw new ArgumentNullException("command");

            Scope = scope;
            Command = command;
        }

        /// <summary>
        /// Scope that the command was invoked in (scope is still open).
        /// </summary>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Command that was executed.
        /// </summary>
        public Command Command { get; private set; }
    }
}