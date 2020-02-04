using System.Net;
using System.Net.Sockets;

namespace Griffin.Net
{
    public class MessagingServerHandlerFactoryContext
    {
        public EndPoint EndPoint { get; set; }
        public Socket Socket { get; set; }
    }
}