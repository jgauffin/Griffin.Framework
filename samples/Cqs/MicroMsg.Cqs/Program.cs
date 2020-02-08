using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        static async Task Main(string[] args)
        {
            var microServer = CreateServer();
            microServer.RunAsync(IPAddress.Loopback, 0, CancellationToken.None);

            var client = await CreateClient(microServer.LocalPort);
            client.ExecuteAsync(new HelloWorld()).Wait();

            Console.WriteLine("Hello World!");
        }

        private static MessagingServer<MicroMessageContext> CreateServer()
        {
            var microConfig = new MessagingServerConfiguration<MicroMessageContext>
            {
                HandlerFactory = x => new MicroMessageHandler(new DataContractMessageSerializer(), new TcpChannel())
            };
            var microServer = new MessagingServer<MicroMessageContext>(microConfig);
            return microServer;
        }

        private static async Task<CqsClient> CreateClient(int port)
        {
            var client = new CqsClient(() => new DataContractMessageSerializer())
            {
                Authenticator = new HashClientAuthenticator(new NetworkCredential("jonas", "mamma"))
            };
            await client.StartAsync(IPAddress.Loopback, port);
            return client;
        }
    }
}
