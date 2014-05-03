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
    public class SimpleAccountService : IAccountService
    {
        /// <summary>
        /// Lookups the specified user
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="host">Typically web server domain name.</param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <remarks>
        /// User name can basically be anything. For instance name entered by user when using
        /// basic or digest authentication, or SID when using Windows authentication.
        /// </remarks>
        public IAuthenticationUser Lookup(string userName, Uri host)
        {
            return new BasicUser(){Username = "arne", Password = "hej"};
        }

        /// <summary>
        /// Hash password to be able to do comparison
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string userName, Uri host, string password)
        {
            return password;
        }

        public class BasicUser : IAuthenticationUser
        {
            /// <summary>
            /// Gets or sets user name used during authentication.
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets unencrypted password.
            /// </summary>
            /// <remarks>
            /// Password as clear text. You could use <see cref="HA1"/> instead if your passwords
            /// are encrypted in the database.
            /// </remarks>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets HA1 hash.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Digest authentication requires clear text passwords to work. If you
            /// do not have that, you can store a HA1 hash in your database (which is part of
            /// the Digest authentication process).
            /// </para>
            /// <para>
            /// A HA1 hash is simply a Md5 encoded string: "UserName:Realm:Password". The quotes should
            /// not be included. Realm is the currently requested Host (as in <c>Request.Headers["host"]</c>).
            /// </para>
            /// <para>
            /// Leave the string as <c>null</c> if you are not using HA1 hashes.
            /// </para>
            /// </remarks>
            public string HA1 { get; set; }
        }
    }
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
