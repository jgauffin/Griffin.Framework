using System;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// An event have been published.
    /// </summary>
    /// <remarks>
    /// <para>Wether all handlers succeeded or not is specified by the <see cref="Successful"/> property.</para>
    /// </remarks>
    public class EventPublishedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventPublishedEventArgs"/> class.
        /// </summary>
        /// <param name="scope">Scope used to resolve subscribers.</param>
        /// <param name="applicationEvent">Published event.</param>
        /// <param name="successful">All handlers processed the event successfully.</param>
        /// <exception cref="System.ArgumentNullException">
        /// scope
        /// or
        /// applicationEvent
        /// </exception>
        public EventPublishedEventArgs(IContainerScope scope, ApplicationEvent applicationEvent, bool successful)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (applicationEvent == null) throw new ArgumentNullException("applicationEvent");

            Scope = scope;
            ApplicationEvent = applicationEvent;
            Successful = successful;
        }

        /// <summary>
        /// Scope used to resolve subscribers
        /// </summary>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Published event
        /// </summary>
        public ApplicationEvent ApplicationEvent { get; private set; }

        /// <summary>
        /// <c>true</c> = None of the subscribers failed.
        /// </summary>
        public bool Successful { get; private set; }
    }
}