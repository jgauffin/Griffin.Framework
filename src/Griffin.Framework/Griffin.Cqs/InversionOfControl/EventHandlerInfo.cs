using System;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// Contains information about all publishers
    /// </summary>
    public class EventHandlerInfo
    {
        public EventHandlerInfo(Type subscriberType, long invocationTime)
        {
            if (subscriberType == null) throw new ArgumentNullException("subscriberType");
            SubscriberType = subscriberType;
            InvocationTime = invocationTime;
        }

        /// <summary>
        /// Subscriber type
        /// </summary>
        public Type SubscriberType { get; set; }

        /// <summary>
        /// Amount of time (in milliseconds) used to handle the event.
        /// </summary>
        public long InvocationTime { get; set; }
    }
}