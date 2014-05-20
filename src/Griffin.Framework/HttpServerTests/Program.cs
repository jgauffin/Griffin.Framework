using System;
using System.IO;
using System.Net;
using System.Text;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using Griffin.Net.Protocols.Http.BodyDecoders;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;

namespace HttpServerTests
{
    class Program
    {
        static IAuthenticator _authenticator = new BasicAuthentication(new SimpleAccountService(), "griffin");
        static void Main(string[] args)
        {
            var listener = new HttpListener();
            listener.ClientConnected += OnConnect;
            listener.MessageReceived = OnMessage;
            listener.BodyDecoder = new CompositeBodyDecoder();
            listener.Start(IPAddress.Any, 8083);

            Console.ReadLine();
        }

        

        private static void OnConnect(object sender, ClientConnectedEventArgs e)
        {
            
        }

        private static void OnMessage(ITcpChannel channel, object message)
        {
            Console.WriteLine(message);
            var request = (HttpRequestBase)message;
            var response = request.CreateResponse();

            if (request.Uri.AbsolutePath.StartsWith("/restricted"))
            {

                var user = _authenticator.Authenticate(request);
                if (user == null)
                {
                    _authenticator.CreateChallenge(request, response);
                    channel.Send(response);
                    return;
                }
                    

                Console.WriteLine("Logged in: " + user);
            }
            
            var msg = Encoding.UTF8.GetBytes("Hello world");
            if (request.Uri.ToString().Contains(".jpg"))
            {
                response.Body = new FileStream(@"C:\users\gaujon01\Pictures\DSC_0231.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                response.ContentType = "image/jpeg";
            }
            else
            {
                response.Body = new MemoryStream();
                response.Body.Write(msg, 0, msg.Length);
                response.ContentType = "text/plain";
            }
            channel.Send(response);
        }
    }
}
