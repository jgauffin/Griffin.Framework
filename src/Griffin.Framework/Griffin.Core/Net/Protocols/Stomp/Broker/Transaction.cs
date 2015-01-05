using System;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// A batch of messages
    /// </summary>
    public class Transaction
    {
        LinkedList<Action> _commitTasks = new LinkedList<Action>();
        LinkedList<Action> _rollbackTasks = new LinkedList<Action>();


        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <exception cref="System.ArgumentNullException">transactionId</exception>
        public Transaction(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");
            Id = transactionId;
        }

        /// <summary>
        /// Transaction identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Add a state subscriber
        /// </summary>
        /// <param name="commitTask">Action to execute once committing</param>
        /// <param name="rollbackTask">Action to take if rolling back</param>
        /// <exception cref="System.ArgumentNullException">
        /// commitTask
        /// or
        /// rollbackTask
        /// </exception>
        public void Add(Action commitTask, Action rollbackTask)
        {
            if (commitTask == null) throw new ArgumentNullException("commitTask");
            if (rollbackTask == null) throw new ArgumentNullException("rollbackTask");
            _commitTasks.AddLast(commitTask);
            _rollbackTasks.AddLast(rollbackTask);
        }

        /// <summary>
        /// Rollback transaction.
        /// </summary>
        public void Rollback()
        {
            foreach (var task in _rollbackTasks)
            {
                task();
            }

        }

        /// <summary>
        /// Commit transaction.
        /// </summary>
        public void Commit()
        {
            foreach (var task in _commitTasks)
            {
                task();
            }
        }
    }
}