using System;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public class Transaction
    {
        LinkedList<Action> _commitTasks = new LinkedList<Action>();
        LinkedList<Action> _rollbackTasks = new LinkedList<Action>();


        public Transaction(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");
            Id = transactionId;
        }

        public string Id { get; set; }

        public void Add(Action commitTask, Action rollbackTask)
        {
            _commitTasks.AddLast(commitTask);
            _rollbackTasks.AddLast(rollbackTask);
        }

        public void Rollback()
        {
            foreach (var task in _rollbackTasks)
            {
                task();
            }

        }

        public void Commit()
        {
            foreach (var task in _commitTasks)
            {
                task();
            }
        }
    }
}