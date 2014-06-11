using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Griffin.Cqs.Demo.Command;
using Griffin.Cqs.Net;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs.Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            var cmdBus = new SimpleCommandBus();
            cmdBus.Register(typeof(Program).Assembly);

            var queryBus = new SimpleQueryBus();
            queryBus.Register(typeof(Program).Assembly);

            var requestReplyBus = new SimpleRequestReplyBus();
            requestReplyBus.Register(typeof(Program).Assembly);

            var eventBus = new SimpleEventBus();
            eventBus.Register(typeof(Program).Assembly);

            var server = new CqsServer(cmdBus, queryBus, eventBus, requestReplyBus);
            server.Start(IPAddress.Any, 0);

            var client = new CqsClient(IPAddress.Loopback, server.LocalPort);
            client.ExecuteAsync(new IncreaseDiscount(20)).Wait();

            //server.Start();

            cmdBus.ExecuteAsync(new IncreaseDiscount()).Wait();

            Console.ReadLine();

        }
    }
}
