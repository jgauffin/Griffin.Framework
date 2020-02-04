using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Channels;
using Griffin.Net.LiteServer.Modules;
using Griffin.Net.LiteServer.Modules.Authentication;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Serializers;

namespace HttpServerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var certificate = new X509Certificate2("GriffinNetworkingTemp.pfx", "mamma");

            var configuration = new HttpConfiguration {Certificate = certificate};
            configuration.Pipeline.Register(new MustAlwaysAuthenticate());
            configuration.Pipeline.Register(new MyHttpMiddleware());

            var server = new HttpServer(configuration);
            server.RunAsync(IPAddress.Any, CancellationToken.None);

            Console.ReadLine();
        }

        private void TryMicroMsg()
        {
            var microConfig = new MessagingServerConfiguration<MicroMessageContext>();
            microConfig.HandlerFactory = x => new MicroMessageHandler(new DataContractMessageSerializer(), new TcpChannel());
            var microServer = new MessagingServer<MicroMessageContext>(microConfig);

            var client = new CqsClient(() => new DataContractMessageSerializer());
            client.Authenticator = new HashClientAuthenticator(new NetworkCredential("jonas", "mamma"));
            client.StartAsync(IPAddress.Loopback, microServer.LocalPort).Wait();
            client.ExecuteAsync(new HelloWorld()).Wait();


        }

    }

}
