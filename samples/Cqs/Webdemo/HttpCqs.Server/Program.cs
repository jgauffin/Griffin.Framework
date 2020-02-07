using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Griffin.Core.Autofac;
using Griffin.Cqs;
using Griffin.Cqs.Authorization;
using Griffin.Cqs.InversionOfControl;
using Griffin.Http;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware.Authentication.Basic;
using Griffin.Net.Protocols.Http.Middleware.DependencyInjection;
using Griffin.Net.Protocols.Http.WebSocket;

namespace HttpCqs.Server
{
    internal class Program
    {
        private static AutofacAdapter BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterServices(Assembly.GetExecutingAssembly());
            var container = containerBuilder.Build();
            var adapter = new AutofacAdapter(container);
            return adapter;
        }

        static async Task Main(string[] args)
        {
            // tell library to authorize the usage of command/queries
            GlobalConfiguration.AuthorizationFilter = new RoleAuthorizer();

            // build autofac
            var adapter = BuildContainer();

            var builder = new IocBusBuilder(adapter);
            var processor = builder.BuildMessageProcessor();

            var websocketPipeline = new MessagingServerPipeline<WebsocketContext>();

            var httpPipeline = new MessagingServerPipeline<HttpContext>();
            httpPipeline.Register(new DependencyInjectionMiddleware<HttpContext>(adapter));
            httpPipeline.Register(new BasicAuthentication(new MyAccountService(), "My realm"));
            httpPipeline.Register(new FileMiddleware());
            httpPipeline.Register(new CqsMiddleware(processor));
            //httpPipeline.Register(new MySimpleServer());


            var bufferManager = new BufferManager(10, 65535);
            var config = new MessagingServerConfiguration<HttpContext>
            {
                HandlerFactory = context =>
                {
                    var channel = new TcpChannel();
                    channel.Assign(context.Socket);
                    return new WebSocketHandler(channel, bufferManager.Dequeue(), httpPipeline, websocketPipeline);
                }
            };

            var server = new MessagingServer<HttpContext>(config);
            await server.RunAsync(IPAddress.Any, 8080, CancellationToken.None);

            Console.WriteLine("Browse to http://localhost:8899 and login with user/user or admin/admin.");
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();

            await server.StopAsync();
        }
    }
}