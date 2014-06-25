using System;
using System.Net;
using System.Security.Principal;
using Griffin.Net.Channels;

namespace Griffin.Net.Server.Modules
{
    /// <summary>
    /// Information required for modules to be able to do their processing.
    /// </summary>
    public class ClientContext : IClientContext
    {
        private ITcpChannel _channel;

        public ClientContext(ITcpChannel channel, object message)
        {
            if (channel == null) throw new ArgumentNullException("channel");
            _channel = channel;
            RequestMessage = message;
            ChannelData = Channel.Data;
            RemoteEndPoint = channel.RemoteEndpoint;
            RequestData = new DictionaryContextData();
        }

        /// <summary>
        /// Address to the currently connected client.
        /// </summary>
        public EndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// Inbound message
        /// </summary>
        public object RequestMessage { get; set; }

        /// <summary>
        /// Message to send to the client
        /// </summary>
        public object ResponseMessage { get; set; }

        /// <summary>
        /// Can be compared with a session in a web server.
        /// </summary>
        public IChannelData ChannelData { get; private set; }

        /// <summary>
        /// Data specific for this request
        /// </summary>
        public IContextData RequestData { get; private set; }

        /// <summary>
        /// User if authenticated (otherwise <c>GenericPrincipal</c> which is marked as not authenticated)
        /// </summary>
        public IPrincipal User { get; set; }

        /// <summary>
        /// Something failed during processing.
        /// </summary>
        public Exception Error { get; set; }

        internal ITcpChannel Channel
        {
            get { return _channel; }
        }
    }
}