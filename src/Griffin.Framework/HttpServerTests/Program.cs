using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Buffers;
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

            MessagingServerPipeline<HttpContext> pipeline = new MessagingServerPipeline<HttpContext>();
            //pipeline.Register(new HashAuthenticationMiddleware(new FakeFetcher()));
            pipeline.Register(new MustAlwaysAuthenticate());

            BufferManager mgr = new BufferManager(5);
            var config = new MessagingServerConfiguration<HttpContext>
            {
                HandlerFactory = x => new HttpHandler(x.Socket, mgr.Dequeue(), pipeline)
            };

            var server = new MessagingServer<HttpContext>(config);
            var task = server.RunAsync(IPAddress.Any, 0, CancellationToken.None);

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
