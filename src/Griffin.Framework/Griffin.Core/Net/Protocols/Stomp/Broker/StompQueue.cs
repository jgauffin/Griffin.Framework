using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// 
    /// </summary>
    public class StompQueue : IStompQueue
    {
        private readonly ITransactionManager _transactionManager;
        LinkedList<IFrame> _messages = new LinkedList<IFrame>();
        ReaderWriterLockSlim _queueLock = new ReaderWriterLockSlim();
        readonly List<Subscription> _subscriptions = new List<Subscription>();
        ConcurrentQueue<Subscription> _idleSubscriptions = new ConcurrentQueue<Subscription>();
        private Thread _workThread;

        private ManualResetEventSlim _messageEvent = new ManualResetEventSlim();

        public StompQueue()
        {
            _workThread = new Thread(OnSendNextMessage);
            _workThread.Start();
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
                //TODO: Log
            }
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
                Requeue(new []{frame});
            }

            subscription.Send(frame);
        }


        /// <summary>
        /// Will put all frames first in the queue again
        /// </summary>
        /// <param name="frame"></param>
        /// <remarks>
        /// Should only be used for queues that got one client and where the message ordering is important.
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

        

        public string Name { get; set; }

        /// <summary>
        /// Put a message in our queue. 
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>
        /// Messages within transactions will be put on hold until the transaction is commited.
        /// 
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

     
        public void AddSubscription(Subscription subscription)
        {
            subscription.BecameIdle += OnSubscriptionWentIdle;
        }

        private void OnSubscriptionWentIdle(object sender, EventArgs e)
        {
            
        }

        public void Unbsubscribe(Subscription subscription)
        {
            throw new NotImplementedException();
        }
    }
}