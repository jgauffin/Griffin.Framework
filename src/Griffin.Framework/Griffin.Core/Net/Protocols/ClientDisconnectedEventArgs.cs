using System;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    ///     Event arguments for <see cref="ProtocolTcpListener.ClientDisconnected" />.
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDisconnectedEventArgs"/> class.
        /// </summary>
        /// <param name="channel">The channel that disconnected.</param>
        /// <param name="exception">The exception that was caught.</param>
        public ClientDisconnectedEventArgs(ITcpChannel channel, Exception exception)
        {
            if (channel == null) throw new ArgumentNullException("channel");
            if (exception == null) throw new ArgumentNullException("exception");

            Channel = channel;
            Exception = exception;
        }

        /// <summary>
        /// Channel that was disconnected
        /// </summary>
        public ITcpChannel Channel { get; private set; }

        /// <summary>
        /// Exception that was caught (is SocketException if the connection failed or if the remote end point disconnected).
        /// </summary>
        /// <remarks>
        /// <c>SocketException</c> with status <c>Success</c> is created for graceful disconnects.
        /// </remarks>
        public Exception Exception { get; private set; }
    }
}