using System;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    ///     Arguments for <see cref="ClientConnection.ReceivedCommand" />.
    /// </summary>
    public class ClientReceivedCommandEventArgs : EventArgs
    {
        public ClientReceivedCommandEventArgs(string clientId, string command, string[] argv)
        {
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