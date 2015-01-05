using System;
using System.Collections.Concurrent;

namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    /// <summary>
    /// Stores queues in memory. 
    /// </summary>
    public class MemoryQueueRepository : IQueueRepository
    {
        ConcurrentDictionary<string, StompQueue>  _queues = new ConcurrentDictionary<string, StompQueue>();

        /// <summary>
        /// Adds a queue to the repository.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <exception cref="System.ArgumentNullException">queue</exception>
        /// <exception cref="System.ArgumentException">Queue must have a name;queue.Name</exception>
        public void Add(StompQueue queue)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (string.IsNullOrEmpty(queue.Name)) throw new ArgumentException("Queue must have a name", "queue.Name");

            //TODO: IF queues are added dynamically this could fail
            _queues.TryAdd(queue.Name, queue);
        }


        /// <summary>
        /// Fetch a queue
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        /// <returns>
        /// Queue
        /// </returns>
        /// <exception cref="System.ArgumentNullException">queueName</exception>
        /// <exception cref="Griffin.Net.Protocols.Stomp.Broker.NotFoundException">Queue ' + queueName + ' do not exist</exception>
        public IStompQueue Get(string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");

            StompQueue queue;
            if (!_queues.TryGetValue(queueName, out queue))
                throw new NotFoundException("Queue '" + queueName + "' do not exist");

            return queue;
        }
    }
}
