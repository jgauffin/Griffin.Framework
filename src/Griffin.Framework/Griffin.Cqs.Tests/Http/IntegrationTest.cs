using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Http;
using Griffin.Cqs.Simple;
using Xunit;

namespace Griffin.Cqs.Tests.Http
{
    public class IntegrationTest
    {

        [Fact]
        public async Task Try_server_and_client_together()
        {
            //can be an ioc bus too.
            var commandBus = new SimpleCommandBus();
            commandBus.Register(Assembly.GetExecutingAssembly());

            //takes care of execution
            var processor = new CqsMessageProcessor { CommandBus = commandBus };

            //receive through HTTP
            var server = new CqsHttpListener(processor);
            server.Map(typeof(RaiseHands));
            server.Start(new IPEndPoint(IPAddress.Loopback, 0));

            //Our cqs HTTP client.
            var client = new CqsHttpClient("http://localhost:" + server.LocalPort);
            await client.ExecuteAsync(new RaiseHands { Reason = "all YOUR base" });
        }

        [Fact]
        public async Task Try_server_and_client_together_for_queries()
        {
            //can be an ioc bus too.
            var bus = new SimpleQueryBus();
            bus.Register(Assembly.GetExecutingAssembly());

            //takes care of execution
            var processor = new CqsMessageProcessor { QueryBus = bus };

            //receive through HTTP
            var server = new CqsHttpListener(processor);
            server.Map(typeof(GetUsers));
            server.Start(new IPEndPoint(IPAddress.Loopback, 0));

            //Our cqs HTTP client.
            var client = new CqsHttpClient("http://localhost:" + server.LocalPort);
            var result = await client.QueryAsync(new GetUsers { FirstName = "Jonas" });
        }

        [Fact]
        public async Task Send_command_using_regular_http()
        {
            var server = CreateServer();
            server.Map(typeof(RaiseHands));
            server.Start(new IPEndPoint(IPAddress.Loopback, 0));

            //Regular HTTP
            var client = new HttpClient();
            var content = new StringContent(@"{ ""Reason"": ""So simple!"" }", Encoding.UTF8, "application/json");
            content.Headers.Add("X-Cqs-Name", "RaiseHands");
            await client.PutAsync("http://localhost:" + server.LocalPort, content);
        }


        [Fact]
        public async Task Send_query_using_regular_http()
        {
            var server = CreateServer();
            server.Map(typeof(GetUsers));
            server.Start(new IPEndPoint(IPAddress.Loopback, 0));

            //Regular HTTP
            var client = new HttpClient();
            var content = new StringContent(@"{ ""FirstName"": ""Jonas"" }", Encoding.UTF8, "application/json");
            content.Headers.Add("X-Cqs-Name", "RaiseHands");
            await client.PostAsync("http://localhost:" + server.LocalPort, content);
        }

        private static CqsHttpListener CreateServer()
        {
            //can be an ioc bus too.
            var commandBus = new SimpleCommandBus();
            commandBus.Register(Assembly.GetExecutingAssembly());

            var queryBus = new SimpleQueryBus();
            queryBus.Register(Assembly.GetExecutingAssembly());

            //takes care of execution
            var processor = new CqsMessageProcessor { CommandBus = commandBus, QueryBus = queryBus };

            //receive through HTTP
            var server = new CqsHttpListener(processor);
            return server;
        }
    }


    public class RaiseHands : Command
    {
        public string Reason { get; set; }
    }

    public class RaiseHandsHandler : ICommandHandler<RaiseHands>
    {
        public async Task ExecuteAsync(RaiseHands command)
        {

        }
    }

    public class GetUsers : Query<GetUsersResult>
    {
        public string FirstName { get; set; }
    }

    public class GetUsersResult
    {
        public int Count { get; set; }
    }

    public class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
    {
        public async Task<GetUsersResult> ExecuteAsync(GetUsers command)
        {
            return new GetUsersResult() { Count = 10 };
        }
    }
}
