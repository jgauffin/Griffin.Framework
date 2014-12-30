namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    ///     A queue contract for the STOMP protocol
    /// </summary>
    public interface IStompQueue
    {
        /// <summary>
        ///     Name of the queue as defined by the standard specification
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Put a message in our queue.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>
        ///     Messages within transactions will be put on hold until the transaction is commited.
        /// </remarks>
        void Enqueue(IFrame message);

        /// <summary>
        /// Add a new subscription
        /// </summary>
        /// <param name="subscription"></param>
        void AddSubscription(Subscription subscription);

        /// <summary>
        /// Unsubscribe on this queue.
        /// </summary>
        /// <param name="subscription"></param>
        void Unsubscribe(Subscription subscription);
    }
}