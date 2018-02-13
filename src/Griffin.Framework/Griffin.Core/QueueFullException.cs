using System;
using System.Runtime.Serialization;

namespace Griffin
{
    /// <summary>
    ///     Queue is full and no more items may be enqueued.
    /// </summary>
    public class QueueFullException : Exception
    {
        private readonly string _queueName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        public QueueFullException(string queueName)
            : base("Queue '" + queueName + "' is full.")
        {
            _queueName = queueName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="inner">The inner.</param>
        public QueueFullException(string queueName, Exception inner)
            : base("Queue '" + queueName + "' is full.", inner)
        {
        }
        
        /// <summary>
        ///     Name of the queue that is full.
        /// </summary>
        public string QueueName
        {
            get { return _queueName; }
        }
        
    }
}