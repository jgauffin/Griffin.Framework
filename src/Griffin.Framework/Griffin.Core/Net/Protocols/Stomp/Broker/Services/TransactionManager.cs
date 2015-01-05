using System;
using System.Collections.Concurrent;

namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    /// <summary>
    /// Takes care of transactions for a specific connection.
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private ConcurrentDictionary<string, Transaction> _transactions =
            new ConcurrentDictionary<string, Transaction>();

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        /// <param name="transactionId">Identifier. Must be unique within the same client connection</param>
        /// <exception cref="System.InvalidOperationException">Transaction ' + transactionId + ' have already been started.</exception>
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

        /// <summary>
        /// Enqueue work in an existing transaction (i.e. you must have invoked <c>BeginTransaction()</c> first)
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="commitTask">Action that will be invoked when the transaction is committed.</param>
        /// <param name="rollbackTask"></param>
        /// <exception cref="System.ArgumentNullException">
        /// transactionId
        /// or
        /// commitTask
        /// or
        /// rollbackTask
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Transaction ' + transactionId +
        /// ' do not exist (or have been committed/rolled back).</exception>
        public void Enqueue(string transactionId, Action commitTask, Action rollbackTask)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");
            if (commitTask == null) throw new ArgumentNullException("commitTask");
            if (rollbackTask == null) throw new ArgumentNullException("rollbackTask");
            Transaction transaction;
            if (!_transactions.TryGetValue(transactionId, out transaction))
                throw new InvalidOperationException("Transaction '" + transactionId +
                                                    "' do not exist (or have been committed/rolled back).");

            transaction.Add(commitTask, rollbackTask);
        }

        /// <summary>
        /// Rollback a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="Begin" />.</param>
        /// <exception cref="System.ArgumentNullException">transactionId</exception>
        /// <exception cref="System.InvalidOperationException">Transaction ' + transactionId +
        ///                                                     ' do not exist (or have been committed/rolled back).</exception>
        public void Rollback(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");
            Transaction transaction;
            if (!_transactions.TryRemove(transactionId, out transaction))
                throw new InvalidOperationException("Transaction '" + transactionId +
                                                    "' do not exist (or have been committed/rolled back).");

            transaction.Rollback();
        }

        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="Begin" />.</param>
        /// <exception cref="System.ArgumentNullException">transactionId</exception>
        /// <exception cref="System.InvalidOperationException">Transaction ' + transactionId +
        ///                                                     ' do not exist (or have been committed/rolled back).</exception>
        public void Commit(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");
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
        ///     Determines if the connection have active transactions.
        /// </summary>
        public bool HasActiveTransactions
        {
            get { return _transactions.Count > 0; }
        }

    }
}