using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class Issue14ShouldWorkWithConcurrentRequests : HttpMiddleware, IDisposable
    {
        private HttpServer _server;
        ManualResetEvent _eventToTriggerBySecond = new ManualResetEvent(false);

        public Issue14ShouldWorkWithConcurrentRequests()
        {
            var config = new HttpConfiguration {Port = 0};
            config.Pipeline.Register(this);
            _server = new HttpServer(config);
            _server.RunAsync(IPAddress.Loopback, CancellationToken.None);
        }

        [Fact(Skip = "Needs a timeout")]
        public void InvokeTwoRequests_Both_Should_succeed()
        {
            var httpMsg = @"GET / HTTP/1.0
Host:coderr.io
Content-Length: 0

";
            var client1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client1.Connect(IPAddress.Loopback, _server.LocalPort);
            var client2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client2.Connect(IPAddress.Loopback, _server.LocalPort);


            client1.Send(Encoding.ASCII.GetBytes(httpMsg));
            client2.Send(Encoding.ASCII.GetBytes(httpMsg));

            var buf = new byte[65535];
            var sw = new Stopwatch();
            sw.Start();
            client2.Receive(buf);
            sw.Stop();

            sw.ElapsedMilliseconds.Should().BeLessThan(500);

            //ServicePointManager.FindServicePoint(new Uri("http://localhost:" + _server.LocalPort)).ConnectionLimit = 5;
            //var request = HttpWebRequest.CreateHttp("http://localhost:" + _server.LocalPort);
            //var arForFirstRequest = request.BeginGetResponse(null, null);

            //var secondRequestTimer = new Stopwatch();
            //secondRequestTimer.Start();
            //var request2 = WebRequest.CreateHttp("http://localhost:" + _server.LocalPort);
            //request2.GetResponse();
            //secondRequestTimer.Stop();

            //request.EndGetResponse(arForFirstRequest);

            //secondRequestTimer.ElapsedMilliseconds.Should()
            //    .BeLessThan(500, "because it should complete before the timeout of the first request");
        }

        public void Dispose()
        {
            _server.StopAsync();
        }

        public override Task Process(HttpContext context, Func<Task> next)
        {
            if (context.ChannelData["lock"] == null)
            {
                Thread.Sleep(2000);
                context.ChannelData["lock"] = new object();
            }
            else
                _eventToTriggerBySecond.Set();

            return next();
        }
    }
}
