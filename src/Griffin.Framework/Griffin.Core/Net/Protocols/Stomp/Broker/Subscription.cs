using System;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public class Subscription
    {
        private List<IFrame> _pendingFrames = new List<IFrame>();
        private DateTime _startThrottle;
        private int _messagesSentThisSecond;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client">Client that the subscription belongs to</param>
        /// <param name="id">Arbitary string as specified by the client.</param>
        public Subscription(IStompClient client, string id)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (id == null) throw new ArgumentNullException("id");

            Id = id;
            Client = client;
            MaxMessagesPerSecond = 5;
        }

        /// <summary>
        /// Arbitary string as specified by the client.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the queue.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// How each message should be acknowledged by the subscribing client
        /// </summary>
        /// <remarks>
        /// <c>client</c> = accumulative acknowledgment (i.e. all messages up to the specified one is ACK:ed). <c>client-individual</c>, each specific message must be acked. <c>auto</c> = All messages are
        /// considered ACK:ed as soon as they are sent.
        /// </remarks>
        public string AckType { get; set; }

        private IStompClient Client { get; set; }

        public bool IsPending
        {
            get
            {
                return _pendingFrames.Count > 0 && AckType == "client-individual";
            }
        }

        public bool IsReady
        {
            get
            {
                return !IsPending && !IsThrottled;
            }
        }

        public bool IsThrottled
        {
            get
            {
                if (_messagesSentThisSecond >= MaxMessagesPerSecond)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Amount of messages which can be sent to a client per second.
        /// </summary>
        public int MaxMessagesPerSecond { get; set; }

        public void Send(IFrame frame)
        {
            if (frame == null) throw new ArgumentNullException("frame");
            if (frame.Name != "MESSAGE")
                throw new InvalidOperationException("Only MESSAGE frames may be sent through a subscription.");

            _messagesSentThisSecond++;

            if (IsPending)
                throw new InvalidOperationException("Is either waiting on an ACK/NACK for the current message, or you've tried to send too many messages per second without acking them.");
            if (IsThrottled)
                throw new InvalidOperationException("You've tried to send too many messages per second. Adjust the MaxMessagesPerSecond property.");
            if (_pendingFrames.Count >= 20)
                throw new InvalidOperationException("Client already have more then 20 pending messages. Start ACK them.");

            // not 100% accurate, but should keep the throttling reasonable stable.
            if (DateTime.Now.Subtract(_startThrottle).TotalMilliseconds > 1000)
            {
                _messagesSentThisSecond = 0;
                _startThrottle = DateTime.Now;
            }

            if (AckType != "auto")
                _pendingFrames.Add(frame);

            Client.Send(frame);
        }

        public void Ack(string id)
        {
            for (int i = 0; i < _pendingFrames.Count; i++)
            {
                var msgId = _pendingFrames[i].Headers["message-id"];
                if (msgId == id)
                {
                    _pendingFrames.RemoveRange(0, i + 1);
                    return;
                }
            }
        }

        public virtual IEnumerable<IFrame> Nack(string id)
        {
            for (int i = 0; i < _pendingFrames.Count; i++)
            {
                var msgId = _pendingFrames[i].Headers["message-id"];
                yield return _pendingFrames[i];
                if (msgId == id)
                {
                    _pendingFrames.RemoveRange(0, i + 1);
                    yield break;
                }
            }
        }

        public bool IsMessagePending(string messageId)
        {
            foreach (var frame in _pendingFrames)
            {
                if (frame.Headers["message-id"] == messageId)
                    return true;
            }

            return false;
        }

        public event EventHandler BecameIdle;
    }

    public interface ISubscriptionSubscriber
    {
        void OnMessageExpired();
    }
}