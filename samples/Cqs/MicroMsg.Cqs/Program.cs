using System;
using System.Net;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Channels;
using Griffin.Net.LiteServer.Modules;
using Griffin.Net.LiteServer.Modules.Authentication;
using Griffin.Net.Protocols.Serializers;
using MicroMsg.Cqs.Messages;

namespace MicroMsg.Cqs
{
    class Program
    {
        static void Main(string[] args)
        {
            var microConfig = new MessagingServerConfiguration<MicroMessageContext>();
            microConfig.HandlerFactory = x => new MicroMessageHandler(new DataContractMessageSerializer(), new TcpChannel());
            var microServer = new MessagingServer<MicroMessageContext>(microConfig);

            var client = new CqsClient(() => new DataContractMessageSerializer());
            client.Authenticator = new HashClientAuthenticator(new NetworkCredential("jonas", "mamma"));
            client.StartAsync(IPAddress.Loopback, microServer.LocalPort).Wait();
            client.ExecuteAsync(new HelloWorld()).Wait();

            Console.WriteLine("Hello World!");
        }
    }
}
