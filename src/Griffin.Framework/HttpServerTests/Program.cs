using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Authentication;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Channels;
using Griffin.Net.LiteServer;
using Griffin.Net.LiteServer.Modules;
using Griffin.Net.LiteServer.Modules.Authentication;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;
using Griffin.Security;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;

namespace HttpServerTests
{
    public class HelloWorld : Command
    {
        
    }

    class Program
    {
        static IAuthenticator _authenticator = new BasicAuthentication(new SimpleAccountService(), "griffin");
        static void Main(string[] args)
        {
            var certificate = new X509Certificate2("GriffinNetworkingTemp.pfx", "mamma");

            var config = new LiteServerConfiguration();
            config.Modules.AddAuthentication(new HashAuthenticationModule(new FakeFetcher()));
            config.Modules.AddAuthorization(new MustAlwaysAuthenticate());
            var server = new LiteServer(config);
            server.Start(IPAddress.Loopback, 0);

            var client = new CqsClient(() => new DataContractMessageSerializer());
            client.Authenticator = new HashClientAuthenticator(new NetworkCredential("jonas", "mamma"));
            client.StartAsync(IPAddress.Loopback, server.LocalPort).Wait();
            client.ExecuteAsync(new HelloWorld()).Wait();


            var listener = new HttpListener();
            listener.ChannelFactory = new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(certificate));
            listener.ClientConnected += OnConnect;
            listener.MessageReceived = OnMessage;
            listener.BodyDecoder = new CompositeIMessageSerializer();
            listener.Start(IPAddress.Any, 8083);

            
            Console.ReadLine();
        }



        private static void OnConnect(object sender, ClientConnectedEventArgs e)
        {
        }


        private static void OnMessage(ITcpChannel channel, object message)
        {
            var request = (HttpRequest)message;
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

            if (request.Uri.AbsolutePath == "/favicon.ico")
            {
                response.StatusCode = 404;
                channel.Send(response);
                return;
            }

            var msg = Encoding.UTF8.GetBytes("Hello world");
            if (request.Uri.ToString().Contains(".jpg"))
            {
                response.Body = new FileStream(@"C:\users\gaujon01\Pictures\DSC_0231.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                response.ContentType = "image/jpeg";
            }
            else
            {
                response.Body = new MemoryStream(msg);
                //response.Body.Write(msg, 0, msg.Length);
                response.ContentType = "text/plain";
            }
            channel.Send(response);
            if (request.HttpVersion == "HTTP/1.0")
                channel.Close();

        }
    }

    internal class MustAlwaysAuthenticate : IServerModule
    {
        public async Task BeginRequestAsync(IClientContext context)
        {
            
        }

        public async Task EndRequest(IClientContext context)
        {
        }

        public async Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            context.ResponseMessage = new AuthenticationRequiredException("Must authenticate");
            return ModuleResult.SendResponse;
        }
    }

    internal class FakeFetcher : IUserFetcher
    {
        PasswordHasherRfc2898 _hasher = new PasswordHasherRfc2898();
        /// <summary>
        ///     Get a user from your data source
        /// </summary>
        /// <param name="userName">
        ///     Some sort of user identity (which the user supplied at client-side). Do not necessarily have to be a user name, but
        ///     could be a email or similar
        ///     depending on how you let users log in.
        /// </param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">userName was not supplied</exception>
        public Task<IUserAccount> FindUserAsync(string userName)
        {
            var salt = _hasher.CreateSalt();
            return Task.FromResult((IUserAccount) new FakeUser()
            {
                HashedPassword = _hasher.HashPassword("mamma", salt),
                PasswordSalt = salt,
                IsLocked = false,
                UserName = "jonas"
            });
        }

        /// <summary>
        ///     Get all roles that the specified user is a member of
        /// </summary>
        /// <param name="account">Account previously fetched using <see cref="IUserFetcher.FindUserAsync" />.</param>
        /// <returns>An array of roles (or an empty array if roles are not used)</returns>
        /// <exception cref="ArgumentException">user is not found</exception>
        /// <exception cref="ArgumentNullException">User was not supplied</exception>
        public Task<string[]> GetRolesAsync(IUserAccount account)
        {
            return Task.FromResult(new string[0]);
        }
    }

    internal class FakeUser : IUserAccount
    {
        /// <summary>
        ///     User identity (as entered by the user during the login process)
        /// </summary>
        public string UserName { get;  set; }

        /// <summary>
        ///     Password hashed in the same was as used by <see cref="IPasswordHasher" />.
        /// </summary>
        public string HashedPassword { get;  set; }

        /// <summary>
        ///     Salt generated when the password was hashed.
        /// </summary>
        public string PasswordSalt { get;  set; }

        /// <summary>
        ///     Account is locked (the user may not login event if the password is correct)
        /// </summary>
        public bool IsLocked { get;  set; }
    }
}
