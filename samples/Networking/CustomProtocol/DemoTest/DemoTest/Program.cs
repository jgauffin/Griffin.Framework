using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using Newtonsoft.Json.Converters;

namespace DemoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ChannelTcpListenerConfiguration(
                () => new MyProtocolDecoder(), 
                () => new MyProtocolEncoder()
            );
            var server = new ChannelTcpListener(config);
            server.MessageReceived += OnServerMessageReceived;
            server.Start(IPAddress.Any, 0);


            ExecuteClient(server).Wait();

            Console.WriteLine("Demo completed");
            Console.ReadLine();

        }

        private static async Task ExecuteClient(ChannelTcpListener server)
        {
            var client = new MyProtocolClient();
            await client.ConnectAsync(IPAddress.Loopback, server.LocalPort);
            await client.SendAsync(new Ping{Name = "TheClient"});
            var response = await client.ReceiveAsync();
            Console.WriteLine("Client received: " + response);
        }

        private static void OnServerMessageReceived(ITcpChannel channel, object message)
        {
            var ping = (Ping) message;
            if (ping == null)
                throw new Exception("Server received unexpected object type.");

            Console.WriteLine("Server received: " + message);
            channel.Send(new Pong
            {
                From = "Server", 
                To = ping.Name
            });
        }
    }
}
