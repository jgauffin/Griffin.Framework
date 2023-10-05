using System;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// Contains information about all message handlers.
    /// </summary>
    public class MessageHandlerInfo
    {
        /// <summary>
        /// Create a new instance of <see cref="MessageHandlerInfo"/>.
        /// </summary>
        /// <param name="subscriberType">Type of class that received the event.</param>
        /// <param name="processingTime">How many milliseconds it too to invoke the handler.</param>
        public MessageHandlerInfo(Type subscriberType, long processingTime)
        {
            SubscriberType = subscriberType ?? throw new ArgumentNullException(nameof(subscriberType));
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