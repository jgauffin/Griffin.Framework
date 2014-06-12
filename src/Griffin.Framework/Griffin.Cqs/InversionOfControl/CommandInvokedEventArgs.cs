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
        public CommandInvokedEventArgs(IContainerScope scope, Command command)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (command == null) throw new ArgumentNullException("command");
            Scope = scope;
            Command = command;
        }

        public IContainerScope Scope { get; private set; }
        public Command Command { get; private set; }
    }
}