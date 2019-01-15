using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Channels;
using Xunit;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class Issue14ShouldWorkWithConcurrentRequests : IDisposable
    {
        private HttpListener _server;
        ManualResetEvent _eventToTriggerBySecond = new ManualResetEvent(false);

        public Issue14ShouldWorkWithConcurrentRequests()
        {
            _server = new HttpListener();
            _server.MessageReceived += OnMessage;
            _server.Start(IPAddress.Any, 0);
        }

        private void OnMessage(ITcpChannel channel, object message)
        {
            if (channel.Data["lock"] == null)
            {
                Thread.Sleep(2000);
                channel.Data["lock"] = new object();
            }
            else
                _eventToTriggerBySecond.Set();
        }

        [Fact(Skip = "Needs a timeout")]
        public void InvokeTwoRequests_Both_Should_succeed()
        {
            var httpMsg = @"GET / HTTP/1.0
Host:onetrueerror.com
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
            _server.Stop();
        }

    }
}
