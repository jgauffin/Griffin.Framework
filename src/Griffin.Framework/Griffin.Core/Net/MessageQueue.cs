using System;
using System.Collections.Concurrent;

namespace Griffin.Net
{
    /// <summary>
    ///     Used to enqueue outbound messages
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implemented using a ConcurrentQueue.
    ///     </para>
    /// </remarks>
    public class MessageQueue : IMessageQueue
    {
        private readonly ConcurrentQueue<object> _outboundMessages = new ConcurrentQueue<object>();

        /// <summary>
        ///     Enqueue a message
        /// </summary>
        /// <param name="message">message to enqueue</param>
        /// <remarks>
        ///     <para>
        ///         Messages do not have to be placed in order, place them as they should be sent out.
        ///     </para>
        /// </remarks>
        public void Enqueue(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _outboundMessages.Enqueue(message);
        }

        /// <summary>
        ///     Get the next message that should be sent
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <returns><c>true</c> if there was a message to send.</returns>
        public bool TryDequeue(out object msg)
        {
            return _outboundMessages.TryDequeue(out msg);
        }
    }
}