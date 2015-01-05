using System;

namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    /// <summary>
    /// Data source for queues.
    /// </summary>
    public interface IQueueRepository
    {
        /// <summary>
        /// Fetch a queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>Queue</returns>
        /// <exception cref="NotFoundException">Queue was not found</exception>
        /// <exception cref="ArgumentNullException">queueName</exception>
        IStompQueue Get(string queueName);
    }
}
