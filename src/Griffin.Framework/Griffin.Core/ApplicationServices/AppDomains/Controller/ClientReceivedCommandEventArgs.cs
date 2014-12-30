using System;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    ///     Arguments for <see cref="ClientConnection.ReceivedCommand" />.
    /// </summary>
    public class ClientReceivedCommandEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientReceivedCommandEventArgs"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="command">The command.</param>
        /// <param name="argv">The argv.</param>
        /// <exception cref="System.ArgumentNullException">clientId</exception>
        public ClientReceivedCommandEventArgs(string clientId, string command, string[] argv)
        {
            if (clientId == null) throw new ArgumentNullException("clientId");
            ClientId = clientId;
            Command = command;
            Argv = argv;
        }

        /// <summary>
        /// App domain identifier
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Command being execited
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Arguments for the command
        /// </summary>
        public string[] Argv { get; private set; }
    }
}