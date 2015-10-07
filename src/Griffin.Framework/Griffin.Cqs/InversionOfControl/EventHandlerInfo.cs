using System;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// Contains information about all publishers
    /// </summary>
    public class EventHandlerInfo
    {
        /// <summary>
        /// Create a new instance of <see cref="EventHandlerInfo"/>.
        /// </summary>
        /// <param name="subscriberType">Type of class that received the event.</param>
        /// <param name="processingTime">How many milliseconds it too to invoke the handler.</param>
        public EventHandlerInfo(Type subscriberType, long processingTime)
        {
            if (subscriberType == null) throw new ArgumentNullException("subscriberType");
            SubscriberType = subscriberType;
            InvocationTime = processingTime;
        }

        /// <summary>
        /// Subscriber type
        /// </summary>
        public Type SubscriberType { get; set; }

        /// <summary>
        /// Amount of time (in milliseconds) used to handle the event.
        /// </summary>
        public long InvocationTime { get; set; }

        /// <summary>
        /// Used internally to mark failures while executing handlers.
        /// </summary>
        internal HandlerFailure Failure { get; set; }
    }
}