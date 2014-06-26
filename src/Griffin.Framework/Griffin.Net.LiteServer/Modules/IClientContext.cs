using System;
using System.Net;
using System.Security.Principal;
using Griffin.Net.Channels;

namespace Griffin.Net.LiteServer.Modules
{
    /// <summary>
    /// Context information about the incoming request.
    /// </summary>
    public interface IClientContext
    {
        /// <summary>
        /// Address to the currently connected client.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Inbound message
        /// </summary>
        object RequestMessage { get; set; }

        /// <summary>
        /// Message to send to the client
        /// </summary>
        object ResponseMessage { get; set; }

        /// <summary>
        /// Can be compared with a session in a web server.
        /// </summary>
        IChannelData ChannelData { get;}

        /// <summary>
        /// Data specific for this request
        /// </summary>
        IContextData RequestData { get;  }

        /// <summary>
        /// User if authenticated (otherwise <c>GenericPrincipal</c> which is marked as not authenticated)
        /// </summary>
        IPrincipal User { get; set; }

        /// <summary>
        /// Something failed during processing.
        /// </summary>
        Exception Error { get; set; }

        
    }
}