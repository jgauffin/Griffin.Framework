namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// Wants to receive events from the subscription
    /// </summary>
    public interface ISubscriptionSubscriber
    {
        /// <summary>
        /// A message has not been delivered within the specified time frame
        /// </summary>
        void OnMessageExpired();
    }
}