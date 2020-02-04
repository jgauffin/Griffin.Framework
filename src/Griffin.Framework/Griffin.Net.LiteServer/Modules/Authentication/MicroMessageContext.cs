using System;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Griffin.Net.Authentication.Messages;
using Griffin.Net.Channels;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    public class MicroMessageContext : IMiddlewareContext
    {
        /// <summary>
        /// Address to the currently connected client.
        /// </summary>
        public EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Inbound message
        /// </summary>
        public object RequestMessage { get; set; }

        /// <summary>
        /// Message to send to the client
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Can be compared with a session in a web server.
        /// </summary>
        public IChannelData ChannelData { get; }

        /// <summary>
        /// User if authenticated (otherwise <c>GenericPrincipal</c> which is marked as not authenticated)
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Something failed during processing.
        /// </summary>
        public Exception Error { get; set; }

    }
}