using System;
using System.Net;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public interface IStompClient
    {
        //event EventHandler<MessageAckedEventArgs> MessageAcked;
        //event EventHandler BecameIdle;

        /// <summary>
        /// Determines if there are one or more active transactions.
        /// </summary>
        bool HasActiveTransactions { get; }

        /// <summary>
        /// Identifier which was created during the authentication process.
        /// </summary>
        /// <remarks>Authentication is always required, no matter if a user/pass is supplied or not.</remarks>
        string SessionKey { get; }

        /// <summary>
        /// Address from where the client is connected.
        /// </summary>
        EndPoint RemoteEndpoint { get; }

        /// <summary>
        /// Check if we are waiting on a ACK/NACK for the specified frame.
        /// </summary>
        /// <param name="messageId">Message id</param>
        /// <returns><c>true</c> if we are waiting on an ack/nack; otherwise <c>false</c>.</returns>
        bool IsFramePending(string messageId);

        /// <summary>
        /// Find our subscription
        /// </summary>
        /// <param name="messageId">A message which should have been distributed using a MESSAGE frame</param>
        /// <returns>Subscription</returns>
        /// <exception cref="NotFoundException">Failed to find a subscription where the message '{0}' is pending (waiting for an ack/nack). Is the subscription really set to use 'client' or 'client-individual' acks?</exception>
        /// <exception cref="System.ArgumentNullException">messageId</exception>
        Subscription GetSubscription(string messageId);

        /// <summary>
        /// Enqueue work in an existing transaction (i.e. you must have invoked <c>BeginTransaction()</c> first)
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="commitAction">Action that will be invoked when the transaction is committed.</param>
        /// <param name="rollbackAction"></param>
        void EnqueueInTransaction(string transactionId, Action commitAction, Action rollbackAction);

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        /// <param name="transactionId">Identifier. Must be unique within the same client connection</param>
        /// <exception cref="InvalidOperationException">Transaction has already been started.</exception>
        void BeginTransaction(string transactionId);

        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="BeginTransaction"/>.</param>
        /// <exception cref="NotFoundException">No transaction have been started with that identifier.</exception>
        void CommitTransaction(string transactionId);

        /// <summary>
        /// Rollback a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="BeginTransaction"/>.</param>
        /// <exception cref="NotFoundException">No transaction have been started with that identifier.</exception>
        void RollbackTransaction(string transactionId);

        /// <summary>
        /// Checks if a subscription have been created (typically using the "SUBSCRIBE" frame).
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier (arbitrary string as specified by the client)</param>
        /// <returns><c>true</c> if the subscription exists; otherwise <c>false</c>.</returns>
        bool SubscriptionExists(string subscriptionId);

        /// <summary>
        /// Add a new subscription
        /// </summary>
        /// <param name="subscription">Subscription that the client requested</param>
        /// <remarks>
        /// The client can have one or more active subscriptions in the same connection. And all subscriptions may have their own AckType. 
        /// </remarks>
        void AddSubscription(Subscription subscription);


        /// <summary>
        /// Remove a subscription (if it exists)
        /// </summary>
        /// <param name="subscriptionId">Identifier as specified when the subscription was created using a "SUBSCRIBE" frame.</param>
        /// <returns>Remove the subscription</returns>
        Subscription RemoveSubscription(string subscriptionId);

        /// <summary>
        /// We have successfully authenticated this client.
        /// </summary>
        void SetAsAuthenticated(string token);

        /// <summary>
        /// Send a frame to the client.
        /// </summary>
        /// <param name="frame">A server side frame</param>
        /// <exception cref="InvalidOperationException">If one attempted to send a client frame.</exception>
        void Send(IFrame frame);
    }
}