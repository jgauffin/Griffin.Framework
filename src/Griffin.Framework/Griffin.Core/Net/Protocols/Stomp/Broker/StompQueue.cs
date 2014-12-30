using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Griffin.Logging;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// </summary>
    public class StompQueue : IStompQueue
    {
        private readonly ConcurrentQueue<Subscription> _idleSubscriptions = new ConcurrentQueue<Subscription>();
        private readonly ManualResetEventSlim _messageEvent = new ManualResetEventSlim();
        private readonly LinkedList<IFrame> _messages = new LinkedList<IFrame>();
        private readonly ReaderWriterLockSlim _queueLock = new ReaderWriterLockSlim();
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly ITransactionManager _transactionManager;
        private Thread _workThread;
        private ILogger _logger = LogManager.GetLogger<StompQueue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StompQueue"/> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        public StompQueue(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
            _workThread = new Thread(OnSendNextMessage);
            _workThread.Start();
        }


        /// <summary>
        /// Queue name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Put a message in our queue.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>
        ///     Messages within transactions will be put on hold until the transaction is commited.
        /// </remarks>
        public void Enqueue(IFrame message)
        {
            if (message == null) throw new ArgumentNullException("message");

            _queueLock.EnterWriteLock();
            try
            {
                _messages.AddLast(message);
            }
            finally
            {
                _queueLock.ExitWriteLock();
            }

            _messageEvent.Set();
        }

        /// <summary>
        /// Stop processing messages in queue
        /// </summary>
        public void Stop()
        {
            _workThread.Join();
        }

        /// <summary>
        /// Add a client subscription
        /// </summary>
        /// <param name="subscription">Subscription that we should deliver messages to.</param>
        public void AddSubscription(Subscription subscription)
        {
            subscription.BecameIdle += OnSubscriptionWentIdle;
        }

        /// <summary>
        /// Unsubscribe on this queue.
        /// </summary>
        /// <param name="subscription"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Unsubscribe(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Will put all frames first in the queue again
        /// </summary>
        /// <param name="frame"></param>
        /// <remarks>
        ///     Should only be used for queues that got one client and where the message ordering is important.
        /// </remarks>
        public void Requeue(IEnumerable<IFrame> frame)
        {
            _queueLock.EnterWriteLock();
            try
            {
                // Required so that they are added in the correct order.
                var it = frame.GetEnumerator();
                it.MoveNext();
                var node = _messages.AddFirst(it.Current);
                while (it.MoveNext())
                {
                    node = _messages.AddAfter(node, it.Current);
                }
            }
            finally
            {
                _queueLock.ExitWriteLock();
            }
        }

        private void OnSendNextMessage()
        {
            try
            {
                while (true)
                {
                    _messageEvent.Wait();
                    SendMessage();

                    _queueLock.EnterReadLock();
                    try
                    {
                        if (_idleSubscriptions.IsEmpty || _messages.Count == 0)
                            _messageEvent.Reset();
                    }
                    finally
                    {
                        _queueLock.ExitReadLock();
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to send a message.", exception);
            }
        }

        private void OnSubscriptionWentIdle(object sender, EventArgs e)
        {
        }

        private void SendMessage()
        {
            IFrame frame;
            _queueLock.EnterWriteLock();
            try
            {
                frame = _messages.First.Value;
                _messages.RemoveFirst();
            }
            finally
            {
                _queueLock.ExitWriteLock();
            }

            Subscription subscription;
            if (!_idleSubscriptions.TryDequeue(out subscription))
            {
                Requeue(new[] {frame});
            }

            subscription.Send(frame);
        }
    }
}