using System;
using System.Collections.Concurrent;

namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    public class TransactionManager : ITransactionManager
    {
        private ConcurrentDictionary<string, Transaction> _transactions =
            new ConcurrentDictionary<string, Transaction>();

        public void Begin(string transactionId)
        {
            Transaction transaction;
            if (_transactions.TryGetValue(transactionId, out transaction))
            {
                throw new InvalidOperationException("Transaction '" + transactionId + "' have already been started.");
            }

            transaction = new Transaction(transactionId);
            _transactions.TryAdd(transactionId, transaction);
        }

        public void Enqueue(string transactionId, Action commitTask, Action rollbackTask)
        {
            Transaction transaction;
            if (!_transactions.TryGetValue(transactionId, out transaction))
                throw new InvalidOperationException("Transaction '" + transactionId +
                                                    "' do not exist (or have been committed/rolled back).");

            transaction.Add(commitTask, rollbackTask);
        }

        public void Rollback(string transactionId)
        {
            Transaction transaction;
            if (!_transactions.TryRemove(transactionId, out transaction))
                throw new InvalidOperationException("Transaction '" + transactionId +
                                                    "' do not exist (or have been committed/rolled back).");

            transaction.Rollback();
        }

        public void Commit(string transactionId)
        {
            Transaction transaction;
            if (!_transactions.TryRemove(transactionId, out transaction))
                throw new InvalidOperationException("Transaction '" + transactionId +
                                                    "' do not exist (or have been committed/rolled back).");

            transaction.Commit();
        }

        /// <summary>
        ///     Remove all existing transactions (rollback all)
        /// </summary>
        public void Cleanup()
        {
            foreach (var transaction in _transactions)
            {
                transaction.Value.Rollback();
            }
        }

        /// <summary>
        ///     Determines if
        /// </summary>
        public bool HasActiveTransactions
        {
            get { return _transactions.Count > 0; }
        }

    }
}