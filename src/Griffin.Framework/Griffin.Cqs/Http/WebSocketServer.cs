using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Cqs.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using Griffin.Net.Protocols.Http.WebSocket;
using Newtonsoft.Json;

namespace Griffin.Cqs.Http
{
    /// <summary>
    /// CQS server that works over WebSockets.
    /// </summary>
    public class CqsWebSocketServer
    {
        private readonly Dictionary<string, Type> _cqsTypes =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private readonly WebSocketListener _listener = new WebSocketListener();
        private readonly CqsMessageProcessor _messageProcessor;
        private Action<string> _logger;
        private Func<ITcpChannel, HttpRequest, HttpResponse> _requestFilter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsHttpListener" /> class.
        /// </summary>
        /// <param name="messageProcessor">Used to execute the actual messages.</param>
        public CqsWebSocketServer(CqsMessageProcessor messageProcessor)
        {
            if (messageProcessor == null) throw new ArgumentNullException("messageProcessor");
            _messageProcessor = messageProcessor;
        }

        /// <summary>
        ///     Assign to get important log messages (typically errors)
        /// </summary>
        public Action<string> Logger
        {
            get { return _logger; }
            set
            {
                if (value == null)
                    _logger = s => { };
                else
                    _logger = value;
            }
        }

        /// <summary>
        ///     Assign to authenticate inbound requests.
        /// </summary>
        public IAuthenticator AuthenticationService { get; set; }

        /// <summary>
        ///     Use to filter inbound requests (or to perform authentication).
        /// </summary>
        /// <returns>
        ///     Response if you stopped the processing; otherwise <c>null</c> to allow this class to contine process the
        ///     inbound message.
        /// </returns>
        public Func<ITcpChannel, HttpRequest, HttpResponse> RequestFilter
        {
            get { return _requestFilter; }
            set
            {
                if (value == null)
                    _requestFilter = (x, y) => null;
                else
                    _requestFilter = value;
            }
        }

        /// <summary>
        ///     Assign if you want to use something else than <c>GenericPrincipal</c>.
        /// </summary>
        public IPrincipalFactory PrincipalFactory { get; set; }

        /// <summary>
        ///     Port that the listener is accepting new connections on.
        /// </summary>
        public int LocalPort
        {
            get { return _listener.LocalPort; }
        }

        /// <summary>
        ///     Map a type directly.
        /// </summary>
        /// <param name="type">Must implement one of the handler interfaces.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="System.ArgumentException">
        ///     ' + type.FullName + ' do not implement one of the handler interfaces.
        ///     or
        ///     ' + type.FullName + ' is abstract or an interface.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        public void Map(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (!IsCqsType(type))
                throw new ArgumentException("'" + type.FullName + "' is not a CQS object.");

            if (_cqsTypes.ContainsKey(type.Name))
                throw new InvalidOperationException(
                    string.Format("Duplicate mappings for name '{0}'. '{1}' and '{2}'.", type.Name, type.FullName,
                        _cqsTypes[type.Name].FullName));

            _cqsTypes.Add(type.Name, type);
        }

        /// <summary>
        ///     Scan assembly for handlers.
        /// </summary>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        /// <param name="assembly">The assembly to scan for handlers.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        public void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!IsCqsType(type))
                    continue;

                if (_cqsTypes.ContainsKey(type.Name))
                    throw new InvalidOperationException(
                        string.Format("Duplicate mappings for name '{0}'. '{1}' and '{2}'.", type.Name, type.FullName,
                            _cqsTypes[type.Name].FullName));

                _cqsTypes.Add(type.Name, type);
            }
        }

        /// <summary>
        ///     Endpoint to listen on.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public void Start(IPEndPoint endPoint)
        {
            _listener.Start(endPoint.Address, endPoint.Port);
            //_listener.MessageReceived += OnMessage;
            _listener.WebSocketClientConnect += OnWebSocketConnect;
            _listener.WebSocketMessageReceived = OnWebsocketMessage;
        }

        private void OnWebsocketMessage(ITcpChannel channel, object message)
        {
            var request = (WebSocketRequest)message;
            if (request.Opcode != WebSocketOpcode.Text)
            {
                Logger("Unexpected msg");
                return;
            }

            var streamReader = new StreamReader(request.Payload);
            var data = streamReader.ReadToEnd();
            var pos = data.IndexOf(':');
            if (pos == -1 || pos > 50)
            {
                Logger("cqsObjectName is missing.");
                return;
            }

            var cqsName = data.Substring(0, pos);
            var json = data.Substring(pos + 1);

            Type type;
            if (!_cqsTypes.TryGetValue(cqsName, out type))
            {
                var response = CreateWebSocketResponse(@"{ ""error"": ""Unknown type: " + cqsName + "\" }");
                channel.Send(response);
                Logger("Unknown type: " + cqsName + ".");
                return;
            }


            var cqs = JsonConvert.DeserializeObject(json, type);
            ClientResponse cqsReplyObject;

            try
            {
                cqsReplyObject = _messageProcessor.ProcessAsync(cqs).Result;
            }
            catch (HttpException ex)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ex.Message,
                    statusCode = ex.HttpCode
                });
                var response = CreateWebSocketResponse(responseJson);
                channel.Send(response);
                return;
            }
            catch (Exception ex)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ex.Message,
                    statusCode = 500
                });
                var response = CreateWebSocketResponse(responseJson);
                channel.Send(response);
                return;
            }


            if (cqsReplyObject.Body is Exception)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ((Exception)cqsReplyObject.Body).Message,
                    statusCode = 500
                });
                var response = CreateWebSocketResponse(responseJson);
                channel.Send(response);
            }
            else
            {
                json = JsonConvert.SerializeObject(cqsReplyObject.Body);
                var reply = CreateWebSocketResponse(json);
                channel.Send(reply);
            }
        }

        private static WebSocketResponse CreateWebSocketResponse(string json)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)) {Position = 0};
            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Text,
                WebSocketMask.Unmask, ms);
            return new WebSocketResponse(frame);
        }

        private void OnWebSocketConnect(object sender, WebSocketClientConnectEventArgs e)
        {
            var success = AuthenticateUser(e.Channel, e.Request);
            if (!success)
                e.CancelConnection();
        }

        private bool AuthenticateUser(ITcpChannel channel, IHttpRequest request)
        {
            if (channel.Data["Principal"] != null)
            {
                Thread.CurrentPrincipal = (IPrincipal)channel.Data["Principal"];
                return true;
            }

            try
            {
                var user = AuthenticationService.Authenticate(request);
                if (user == null)
                    return true;

                if (PrincipalFactory != null)
                {
                    var ctx = new PrincipalFactoryContext(request, user);
                    Thread.CurrentPrincipal = PrincipalFactory.Create(ctx);
                    channel.Data["Principal"] = Thread.CurrentPrincipal;
                    return true;
                }

                var roles = user as IUserWithRoles;
                if (roles == null)
                    throw new InvalidOperationException(
                        "You must specify a PrincipalFactory if you do not return a IUserWithRoles from your IAccountService.");

                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(user.Username), roles.RoleNames);
                channel.Data["Principal"] = Thread.CurrentPrincipal;
            }
            catch (HttpException ex)
            {
                var response = request.CreateResponse();
                response.StatusCode = ex.HttpCode;
                response.ReasonPhrase = FirstLine(ex.Message);
                ;
                channel.Send(response);
                return false;
            }

            return true;
        }

        private string FirstLine(string msg)
        {
            var pos = msg.IndexOfAny(new[] { '\r', '\n' });
            if (pos == -1)
                return msg;

            return msg.Substring(0, pos);
        }

        /// <summary>
        ///     Determines whether the type implements the command handler interface
        /// </summary>
        /// <param name="cqsType">The type.</param>
        /// <returns><c>true</c> if the objects is a command handler; otherwise <c>false</c></returns>
        private static bool IsCqsType(Type cqsType)
        {
            if (cqsType.IsAbstract || cqsType.IsInterface)
                return false;

            if (cqsType.IsSubclassOf(typeof(Command))
                || cqsType.IsSubclassOf(typeof(ApplicationEvent)))
                return true;

            if (cqsType.BaseType == typeof(object))
                return false;

            var typeDef = cqsType.BaseType.GetGenericTypeDefinition();
            if (typeDef == typeof(Query<>)
                || typeDef == typeof(Request<>))
                return true;


            return false;
        }

    }
}
