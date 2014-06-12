using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Command;
using Griffin.Cqs.Demo.Query;
using Griffin.Cqs.Demo.Request;
using Griffin.Cqs.Net;

namespace Griffin.Cqs.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            bool _useIocContainer = true;
            if (_useIocContainer)
            {
                var root = new CompositionRoot();
                root.Build();
            }
            else
            {
                var simpleBuilder = new SimpleCqsBuilder();
                simpleBuilder.Register(Assembly.GetExecutingAssembly());
            }

            var server = new CqsServer(CqsBus.CmdBus,
                CqsBus.QueryBus,
                CqsBus.EventBus,
                CqsBus.RequestReplyBus);
            server.SerializerFactory = () => new JsonMessageSerializer();
            server.Start(IPAddress.Any, 0);

            var client = new CqsClient(() => new JsonMessageSerializer());
            client.StartAsync(IPAddress.Loopback, server.LocalPort).Wait();


            Console.WriteLine("Executing request/reply");
            var t2 = client.ExecuteAsync<LoginReply>(new Login("arne", "mamma"));
            if (t2.Result.Success)
                Console.WriteLine("Logged in successfully");


            Console.WriteLine("Executing command");
            client.ExecuteAsync(new IncreaseDiscount(20)).Wait();

            
            Console.WriteLine("Executing query");
            var t = client.QueryAsync(new GetDiscounts());
            var discounts = t.Result;
            Console.WriteLine("First discount: " + discounts[0].Name);



            Console.ReadLine();
        }
    }
}