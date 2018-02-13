using System;
using System.Collections.Generic;
using DotNetCqs;
using Griffin.Cqs.InversionOfControl;
using Griffin.Cqs.Reliable;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Used by <see cref="QueuedEventBus.HandlerFailed" />.
    /// </summary>
    public class EventHandlerFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerFailedEventArgs"/> class.
        /// </summary>
        /// <param name="applicationEvent">The application event that one or more subscribers failed to process.</param>
        /// <param name="failures">One instance per handler.</param>
        /// <param name="handlerCount">Total amount of subscribers (and not just the amount of failed handlers).</param>
        /// <exception cref="System.ArgumentNullException">
        /// applicationEvent
        /// or
        /// failures
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">handlerCount;Suspicions handler count</exception>
        public EventHandlerFailedEventArgs(ApplicationEvent applicationEvent, IReadOnlyList<HandlerFailure> failures,
            int handlerCount)
        {
            if (applicationEvent == null) throw new ArgumentNullException("applicationEvent");
            if (failures == null) throw new ArgumentNullException("failures");
            if (handlerCount < 0 || handlerCount > 1000)
                throw new ArgumentOutOfRangeException("handlerCount", handlerCount, "Suspicions handler count");

            ApplicationEvent = applicationEvent;
            Failures = failures;
            HandlerCount = handlerCount;
        }

        /// <summary>
        ///     Event that some handlers failed to consume.
        /// </summary>
        public ApplicationEvent ApplicationEvent { get; private set; }

        /// <summary>
        ///     Handlers that failed to consume the event and why they failed
        /// </summary>
        public IReadOnlyList<HandlerFailure> Failures { get; private set; }

        /// <summary>
        ///     Total amount of subscribers (and not just the amount of failed handlers)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Can be used to determine if all or just some handlers failed.
        ///     </para>
        /// </remarks>
        public int HandlerCount { get; private set; }
    }
}