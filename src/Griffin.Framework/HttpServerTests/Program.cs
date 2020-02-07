using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Griffin.Net.Protocols.Http;
using HttpServerTests.Middleware;

namespace HttpServerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var certificate = new X509Certificate2("GriffinNetworkingTemp.pfx", "mamma");

            var configuration = new HttpConfiguration { Certificate = certificate };
            //configuration.Pipeline.Register(new MustAlwaysAuthenticate());
            configuration.Pipeline.Register(new MyHttpMiddleware());

            var server = new HttpServer(configuration);
            configuration.SecurePort = -1;
            configuration.Port = 63772;
            server.RunAsync(IPAddress.Any, CancellationToken.None);

            Console.WriteLine($"Running, surf to http://localhost:{server.LocalPort}/");
            Console.ReadLine();
        }
    }
}
