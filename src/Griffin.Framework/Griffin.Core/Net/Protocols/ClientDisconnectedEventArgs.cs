using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    ///     Event arguments for <see cref="MessagingServer{TContext}.ClientDisconnected" />.
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDisconnectedEventArgs"/> class.
        /// </summary>
        /// <param name="channel">The channel that disconnected.</param>
        /// <param name="exception">The exception that was caught.</param>
        public ClientDisconnectedEventArgs(IBinaryChannel channel, Exception exception)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <summary>
        /// Channel that was disconnected
        /// </summary>
        public IBinaryChannel Channel { get; }

        /// <summary>
        /// Exception that was caught (is SocketException if the connection failed or if the remote end point disconnected).
        /// </summary>
        /// <remarks>
        /// <c>SocketException</c> with status <c>Success</c> is created for graceful disconnects.
        /// </remarks>
        public Exception Exception { get; }
    }
}