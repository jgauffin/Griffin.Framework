using System;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// A command have been successfully invoked
    /// </summary>
    public class EventPublishedEventArgs : EventArgs
    {
        public EventPublishedEventArgs(IContainerScope scope, ApplicationEvent applicationEvent, bool successful)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (applicationEvent == null) throw new ArgumentNullException("applicationEvent");

            Scope = scope;
            ApplicationEvent = applicationEvent;
            Successful = successful;
        }

        public IContainerScope Scope { get; private set; }
        public ApplicationEvent ApplicationEvent { get; private set; }

        /// <summary>
        /// <c>true</c> = None of the subscribers failed.
        /// </summary>
        public bool Successful { get; private set; }
    }
}