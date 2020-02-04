using System;
using System.IO;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    ///     Used by <see cref="MessagingServer{TContext}.ClientConnected" />.
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientConnectedEventArgs" /> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ClientConnectedEventArgs(IBinaryChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            MayConnect = true;
            SendResponse = true;
        }

        /// <summary>
        ///     Channel for the connected client
        /// </summary>
        public IBinaryChannel Channel { get; }

        /// <summary>
        ///     Response (if the client may not connect)
        /// </summary>
        public Stream Response { get; private set; }

        /// <summary>
        ///     Determines if the client may connect.
        /// </summary>
        public bool MayConnect { get; private set; }

        /// <summary>
        ///     Cancel connection, will make the listener close it.
        /// </summary>
        public void CancelConnection()
        {
            MayConnect = false;
        }

        /// <summary>
        /// The library should send a response.
        /// </summary>
        /// <value>
        /// Default is <c>true</c>.
        /// </value>
        public bool SendResponse { get; set; }

        /// <summary>
        ///     Close the listener, but send a response (you are yourself responsible of encoding it to a message)
        /// </summary>
        /// <param name="response">Stream with encoded message (which can be sent as-is).</param>
        public void CancelConnection(Stream response)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
            MayConnect = false;
        }
    }
}