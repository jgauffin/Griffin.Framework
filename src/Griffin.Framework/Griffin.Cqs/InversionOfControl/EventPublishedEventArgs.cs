using System;
using System.Collections.Generic;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     An event have been published.
    /// </summary>
    /// <remarks>
    ///     <para>Whether all handlers succeeded or not is specified by the <see cref="Successful" /> property.</para>
    /// </remarks>
    public class MessagePublishedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagePublishedEventArgs" /> class.
        /// </summary>
        /// <param name="scope">Scope used to resolve subscribers.</param>
        /// <param name="message">Published event.</param>
        /// <param name="successful">All handlers processed the event successfully.</param>
        /// <param name="eventInfo"></param>
        /// <exception cref="System.ArgumentNullException">
        ///     scope
        ///     or
        ///     applicationEvent
        /// </exception>
        public MessagePublishedEventArgs(IContainerScope scope, object message, bool successful,
            IReadOnlyCollection<MessageHandlerInfo> eventInfo)
        {
            Scope = scope;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Successful = successful;
            Handlers = eventInfo;
        }

        /// <summary>
        ///     Scope used to resolve subscribers.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <c>null</c> if one scope is used per handler.
        ///     </para>
        /// </remarks>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        ///     Published event
        /// </summary>
        public object Message { get; private set; }

        /// <summary>
        ///     <c>true</c> = None of the subscribers failed.
        /// </summary>
        public bool Successful { get; private set; }

        /// <summary>
        ///     All subscribers that got invoked for the event.
        /// </summary>
        /// <remarks>
        ///     <para>Subscribers are added in the order that they complete.</para>
        /// </remarks>
        public IReadOnlyCollection<MessageHandlerInfo> Handlers { get; private set; }
    }
}