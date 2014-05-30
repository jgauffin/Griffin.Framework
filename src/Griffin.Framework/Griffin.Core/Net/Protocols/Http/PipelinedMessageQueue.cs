using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     A priority message queue which uses <c>ConcurrentPriorityQueue</c> from Microsoft
    ///     (http://blogs.msdn.com/b/pfxteam/archive/2010/04/04/9990342.aspx)
    /// </summary>
    public class PipelinedMessageQueue : IMessageQueue
    {
        private readonly ConcurrentPriorityQueue<int, object> _queue =
            new ConcurrentPriorityQueue<int, object>();

        private int _lastIndex = 0;

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
            var response = message as IHttpMessage;
            if (response == null)
            {
                _queue.Enqueue(++_lastIndex, message);
                return;
            }

            var header = response.Headers[HttpMessage.PipelineIndexKey];
            if (header == null)
                throw new InvalidOperationException("PipelinedMessageQueue requires the header '" +
                                                        HttpMessage.PipelineIndexKey +
                                                        "' to support HTTP pipelinging.");

            var value = 0;
            if (!int.TryParse(header, out value))
                throw new InvalidOperationException("PipelinedMessageQueue require the header '" +
                                                    HttpMessage.PipelineIndexKey +
                                                    "' and that it contains a numerical value.");

            _lastIndex = value;
            _queue.Enqueue(value, response);
        }

        /// <summary>
        ///     Get the next message that should be sent
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <returns><c>true</c> if there was a message to send.</returns>
        public bool TryDequeue(out object msg)
        {
            KeyValuePair<int, object> kvp;
            if (!_queue.TryDequeue(out kvp))
            {
                msg = null;
                return false;
            }

            msg = kvp.Value;
            return true;
        }
    }
}