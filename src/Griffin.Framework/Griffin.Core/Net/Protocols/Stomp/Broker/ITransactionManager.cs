using System;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// Transaction manager.
    /// </summary>
    /// <remarks>
    /// All transaction ids must only be unique within the scope of a client connection.
    /// </remarks>
    public interface ITransactionManager
    {
        /// <summary>
        /// Begin a new transaction
        /// </summary>
        /// <param name="transactionId">Identifier. Must be unique within the same client connection</param>
        /// <exception cref="InvalidOperationException">Transaction has already been started.</exception>
        void Begin(string transactionId);

        /// <summary>
        /// Enqueue work in an existing transaction (i.e. you must have invoked <c>BeginTransaction()</c> first)
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="commitTask">Action that will be invoked when the transaction is committed.</param>
        /// <param name="rollbackTask"></param>
        void Enqueue(string transactionId, Action commitTask, Action rollbackTask);

        /// <summary>
        /// Rollback a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="Begin"/>.</param>
        /// <exception cref="NotFoundException">No transaction have been started with that identifier.</exception>
        void Rollback(string transactionId);

        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="Begin"/>.</param>
        /// <exception cref="NotFoundException">No transaction have been started with that identifier.</exception>
        void Commit(string transactionId);

        /// <summary>
        /// Remove all existing transactions (rollback all)
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Determines if there are one or more active transactions.
        /// </summary>
        bool HasActiveTransactions { get; }
    }
}