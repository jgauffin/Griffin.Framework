using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Client;
using Griffin.Cqs.Demo.Server;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Server;

namespace Griffin.Cqs.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var server = new ServerDemo();
            server.Setup();
            server.Start();

            var client = new ClientDemo();
            client.RunAsync(server.LocalPort).Wait();

            Console.ReadLine();
        }

    }
}