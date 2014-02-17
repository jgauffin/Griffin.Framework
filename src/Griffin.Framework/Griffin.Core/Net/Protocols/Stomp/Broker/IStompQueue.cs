namespace Griffin.Net.Protocols.Stomp.Broker
{
    public interface IStompQueue
    {
        string Name { get; set; }

        /// <summary>
        /// Put a message in our queue. 
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>
        /// Messages within transactions will be put on hold until the transaction is commited.
        /// 
        /// </remarks>
        void Enqueue(IFrame message);


        void AddSubscription(Subscription subscription);
        void Unbsubscribe(Subscription subscription);
    }
}