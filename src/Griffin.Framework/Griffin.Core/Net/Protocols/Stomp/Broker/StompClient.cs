using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    ///     Implementation of <see cref="IStompClient" />.
    /// </summary>
    public class StompClient : IStompClient
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly ITransactionManager _transactionManager;


        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="transactionManager">
        ///     Must be unique for this client. i.e. the transaction ids used in this client is not
        ///     globally unique
        /// </param>
        public StompClient(ITcpChannel channel, ITransactionManager transactionManager)
        {
            if (channel == null) throw new ArgumentNullException("channel");
            if (transactionManager == null) throw new ArgumentNullException("transactionManager");

            Channel = channel;
            _transactionManager = transactionManager;
        }

        /// <summary>
        ///     Gets if this client has got the CONNECT/STOMP header and got authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        ///     Channel used for communication
        /// </summary>
        private ITcpChannel Channel { get; set; }

        /// <summary>
        ///     Identifier for this specific connection
        /// </summary>
        public string SessionKey { get; private set; }

        /// <summary>
        ///     Address from where the client is connected.
        /// </summary>
        public EndPoint RemoteEndpoint
        {
            get { return Channel.RemoteEndpoint; }
        }

        /// <summary>
        ///     Returns if this client has one or more active transactions
        /// </summary>
        public bool HasActiveTransactions
        {
            get { return _transactionManager.HasActiveTransactions; }
        }

        /// <summary>
        ///     Add a new subscription
        /// </summary>
        /// <param name="subscription">Subscription that the client requested</param>
        /// <exception cref="System.ArgumentNullException">subscription</exception>
        /// <remarks>
        ///     The client can have one or more active subscriptions in the same connection. And all subscriptions may have their
        ///     own AckType.
        /// </remarks>
        public void AddSubscription(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");

            _subscriptions.Add(subscription);
        }

        /// <summary>
        ///     Removes the subscription.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id</exception>
        public Subscription RemoveSubscription(string id)
        {
            if (id == null) throw new ArgumentNullException("id");

            var sub = _subscriptions.FirstOrDefault(x => x.Id == id);
            if (sub != null)
            {
                _subscriptions.Remove(sub);
                return sub;
            }

            return null;
        }

        /// <summary>
        ///     We have successfully authenticated this client.
        /// </summary>
        public void SetAsAuthenticated(string token)
        {
            if (token == null) throw new ArgumentNullException("token");

            SessionKey = token;
        }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction id.</param>
        /// <exception cref="System.ArgumentNullException">transactionId</exception>
        public void BeginTransaction(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");

            _transactionManager.Begin(transactionId);
        }

        /// <summary>
        ///     Rollbacks the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction id.</param>
        /// <exception cref="System.ArgumentNullException">transactionId</exception>
        public void RollbackTransaction(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");

            _transactionManager.Rollback(transactionId);
        }

        /// <summary>
        ///     Commit a transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifier that was specified in <see cref="IStompClient.BeginTransaction" />.</param>
        /// <exception cref="NotFoundException">No transaction have been started with that identifier.</exception>
        public void CommitTransaction(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException("transactionId");

            _transactionManager.Commit(transactionId);
        }

        /// <summary>
        ///     Send a frame to the client.
        /// </summary>
        /// <param name="frame">A server side frame</param>
        /// <exception cref="System.ArgumentNullException">frame</exception>
        /// <exception cref="InvalidOperationException">If one attempted to send a client frame.</exception>
        /// <remarks>
        ///     Messages sent directly through the client are not being modified in any way. If you want to get ack, receipts etc,
        ///     send through a subscription instead.
        /// </remarks>
        public void Send(IFrame frame)
        {
            if (frame == null) throw new ArgumentNullException("frame");

            Channel.Send(frame);
        }

        /// <summary>
        ///     Determines
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public bool IsFramePending(string messageId)
        {
            return _subscriptions.Any(subscription => subscription.IsMessagePending(messageId));
        }

        /// <summary>
        /// Checks if the specified subscription have been added previously.
        /// </summary>
        /// <param name="id">subscription id</param>
        /// <returns><c>true</c> if subscription have been added already; otherwise <c>false</c>.</returns>
        public bool SubscriptionExists(string id)
        {
            return _subscriptions.Any(x => x.Id == id);
        }

        /// <summary>
        /// enqueue client in the specified transaction. I.e. move all pending messages back to queue if transaction fails.
        /// </summary>
        /// <param name="transactionId">Transaction to enlist in</param>
        /// <param name="commitAction">What to do if transaction succeeds</param>
        /// <param name="rollbacAction">What to do if transaction fails</param>
        public void EnqueueInTransaction(string transactionId, Action commitAction, Action rollbacAction)
        {
            _transactionManager.Enqueue(transactionId, commitAction, rollbacAction);
        }

        /// <summary>
        ///     find the subscription where the specified message is pending (i.e. waiting to be acked/nacked).
        /// </summary>
        /// <param name="messageId">The message id.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">messageId</exception>
        /// <exception cref="NotFoundException">
        ///     Failed to find a subscription where the message '{0}' is pending (waiting for an
        ///     ack/nack). Is the subscription really set to use 'client' or 'client-individual' acks?
        /// </exception>
        public Subscription GetSubscription(string messageId)
        {
            if (messageId == null) throw new ArgumentNullException("messageId");
            var sub = _subscriptions.FirstOrDefault(subscription => subscription.IsMessagePending(messageId));
            if (sub == null)
                throw new NotFoundException(
                    string.Format(
                        "Failed to find a subscription where the message '{0}' is pending (waiting for an ack/nack). Is the subscription really set to use 'client' or 'client-individual' acks?",
                        messageId));

            return sub;
        }

        /// <summary>
        /// Reset state for this object (to prepare for handling another connection).
        /// </summary>
        public void Cleanup()
        {
            _transactionManager.Cleanup();
        }

        /// <summary>
        /// Ack all messages before the given sequence number
        /// </summary>
        /// <param name="messageId">Message sequence number</param>
        /// <returns>Subscription that the messages belonged to.</returns>
        public Subscription AckMessages(string messageId)
        {
            if (messageId == null) throw new ArgumentNullException("messageId");

            foreach (var subscription in _subscriptions)
            {
                if (subscription.IsMessagePending(messageId))
                {
                    subscription.Ack(messageId);
                    return subscription;
                }
            }

            throw new NotFoundException(
                string.Format(
                    "Failed to find a MESSSAGE frame with the id of '{0}'. Was it really sent through a subscription?",
                    messageId));
        }

        /// <summary>
        /// Checks of this client is for the specified TCP connection
        /// </summary>
        /// <param name="channel">Connection to take</param>
        /// <returns><c>true</c> if connection is the same; otherwise <c>false.</c></returns>
        public bool IsForChannel(ITcpChannel channel)
        {
            return ReferenceEquals(channel, Channel);
        }
    }
}