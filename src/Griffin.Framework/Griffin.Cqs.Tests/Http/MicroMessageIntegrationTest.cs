using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.Json;
using Griffin.Cqs.Http;
using Griffin.Cqs.Simple;
using Griffin.Cqs.Tests.Http.Helpers;
using Griffin.Http;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Serializers;
using Xunit;

namespace Griffin.Cqs.Tests.Http
{
    public class MicroMessageIntegrationTest
    {
        private static CqsMessageProcessor _processor;
        private static SimpleCommandBus _commandBus;
        private static SimpleQueryBus _queryBus;

        [Fact]
        public async Task Should_be_able_to_execute_a_command()
        {
            var server = CreateServer();
            _commandBus.Register<RaiseHandsHandler, RaiseHands>();
            server.RunAsync(IPAddress.Loopback, CancellationToken.None);

            var client = new CqsHttpClient("http://localhost:" + server.LocalPort);
            await client.ExecuteAsync(new RaiseHands { Reason = "all YOUR base" });

            await server.StopAsync();
        }

        [Fact]
        public async Task Should_Be_able_to_run_a_query()
        {
            var server = CreateServer();
            _queryBus.Register<GetUsersHandler, GetUsers, GetUsersResult>();
            server.RunAsync(IPAddress.Loopback, CancellationToken.None);

            //Our cqs HTTP client.
            var client = new CqsHttpClient("http://localhost:" + server.LocalPort);
            var result = await client.QueryAsync(new GetUsers { FirstName = "Jonas" });

            await server.StopAsync();
            result.Count.Should().Be(10);
        }

        [Fact]
        public async Task Should_be_able_to_plain_JSON()
        {
            var server = CreateServer();
            _commandBus.Register<RaiseHandsHandler, RaiseHands>();
            server.RunAsync(IPAddress.Loopback, CancellationToken.None);

            //Regular HTTP
            var client = new HttpClient();
            var content = new StringContent(@"{ ""Reason"": ""Jonas"" }", Encoding.UTF8, "application/json");
            content.Headers.Add("X-Cqs-Name", "RaiseHands");
            await client.PutAsync("http://localhost:" + server.LocalPort, content, cancellationToken: new CancellationTokenSource(5000).Token);

            await server.StopAsync();
        }


        [Fact]
        public async Task Should_Be_able_to_use_SSL()
        {
            var config = new HttpConfiguration
            {
                Port = -1,
                SecurePort = 0,
                Certificate = new X509Certificate2("GriffinNetworkingTemp.pfx", "mamma")
            };
            var server = CreateServer(config);
            _commandBus.Register<RaiseHandsHandler, RaiseHands>();
            server.RunAsync(IPAddress.Loopback, CancellationToken.None);

            //Regular HTTP
            var client = new HttpClient();
            var content = new StringContent(@"{ ""Reason"": ""Arne"" }", Encoding.UTF8, "application/json");
            content.Headers.Add("X-Cqs-Name", "RaiseHands");
            await client.PostAsync("https://localhost:" + server.SecurePort, content);
        }

        private void Send(int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, port);
            var request= $@"GET / HTTP/1.1
X-Cqs-Name: GetUsers
Content-Type: application/json
Content-Length: 24

{{ ""FirstName"": ""Jonas"" }}";
            var buf = Encoding.UTF8.GetBytes(request);
            socket.Send(buf);
            var buf2 = new byte[65535];
            var rec = socket.Receive(buf2);
            var response = Encoding.UTF8.GetString(buf2, 0, rec);
        }
        private static HttpServer CreateServer(
            HttpConfiguration configuration = null,
            CqsMessageProcessor processor = null)
        {
            var objectMapper = new CqsObjectMapper();

            if (_commandBus == null)
            {
                _commandBus = new SimpleCommandBus(objectMapper);
            }
            if (_queryBus == null)
            {
                _queryBus = new SimpleQueryBus(objectMapper);
            }
            if (processor == null)
            {
                _processor = new CqsMessageProcessor { CommandBus = _commandBus, QueryBus = _queryBus };
            }

            if (configuration == null)
            {
                configuration = new HttpConfiguration
                {
                    Port = 0,
                };
                configuration.ContentSerializers.Clear();
            }

            if (configuration.Pipeline.Count == 0)
            {
                configuration.Pipeline.Register(new CqsMiddleware(_processor, objectMapper));
            }

            return new HttpServer(configuration);
        }
    }
}
