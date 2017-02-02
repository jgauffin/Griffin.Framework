using Griffin.Logging;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Serializers;
using Griffin.Net.Protocols.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin.Routing
{
    /// <summary>
    /// Routed web server class.
    /// </summary>
    public sealed class RoutedWebServer : IDisposable
    {
        private TaskFactory _taskFactory = null;
        private ILogger _logger = null;
        private Net.Protocols.Http.HttpListener _listener = null;
        private int _connectedClients;
        private Router _router = null;
        private List<Func<ITcpChannel, HttpRequest, HttpResponse>> _authMiddleware = null;
        private List<Func<ITcpChannel, HttpRequest, HttpResponse>> _preRoutingMiddleware = null;
        private List<Action<ITcpChannel, HttpRequest, HttpResponse>> _postRoutingMiddleware = null;
        private List<Action<ITcpChannel, HttpRequest, HttpResponse>> _postResponseMiddleware = null;

        /// <summary>
        /// Gets or sets the on404 response factory.
        /// </summary>
        /// <value>The on404 response factory.</value>
        public Func<HttpRequest, HttpResponse> On404 { get; set; }
        /// <summary>
        /// Gets or sets the on401 response factory.
        /// </summary>
        /// <value>The on401 response factory.</value>
        public Func<HttpRequest, HttpResponse> On401 { get; set; }
        /// <summary>
        /// Gets or sets the on403 response factory.
        /// </summary>
        /// <value>The on403 response factory.</value>
        public Func<HttpRequest, HttpResponse> On403 { get; set; }

        private IMessageSerializer bodyDecoder
        {
            get { return _listener.BodyDecoder; }
            set { _listener.BodyDecoder = value; }
        }

        /// <summary>
        /// Gets the connected clients.
        /// </summary>
        /// <value>The connected clients.</value>
        public int ConnectedClients
        {
            get { return _connectedClients; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.RoutedWebServer"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="factory">ControllerFactory.</param>
        public RoutedWebServer(ILogger logger, IControllerFactory factory)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (factory == null)
                throw new ArgumentNullException("factory");

            _logger = logger;
            _connectedClients = 0;
            _router = new Router(factory, (msg) => { _logger.Error("Router->{0}", msg); }, false);
            _taskFactory = new TaskFactory();
            _authMiddleware = new List<Func<ITcpChannel, HttpRequest, HttpResponse>>();
            _preRoutingMiddleware = new List<Func<ITcpChannel, HttpRequest, HttpResponse>>();
            _postRoutingMiddleware = new List<Action<ITcpChannel, HttpRequest, HttpResponse>>();
            _postResponseMiddleware = new List<Action<ITcpChannel, HttpRequest, HttpResponse>>();
        }
        
        /// <summary>
        /// Maps the controller attributes.
        /// </summary>
        public void MapAttributes()
        {
            _router.MapAttributes();
        }

        /// <summary>
        /// Use this function to add Authentication or PreRouting middleware only
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <param name="func"></param>
        public void AddPreMiddleware(MiddlewareEntryPoint entryPoint, Func<ITcpChannel, HttpRequest, HttpResponse> func)
        {
            switch (entryPoint)
            {
                case MiddlewareEntryPoint.Authentication:
                    _authMiddleware.Add(func);
                    break;
                case MiddlewareEntryPoint.PreRouting:
                    _preRoutingMiddleware.Add(func);
                    break;
                case MiddlewareEntryPoint.PostRouting:
                case MiddlewareEntryPoint.PostResponse:
                default:
                    throw new Exception();
                    break;
            }
        }
        /// <summary>
        /// Use this function to add PostRouting and PostResponse middleware only
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <param name="action"></param>
        public void AddPostMiddleware(MiddlewareEntryPoint entryPoint, Action<ITcpChannel, HttpRequest, HttpResponse> action)
        {
            switch (entryPoint)
            {
                case MiddlewareEntryPoint.PostRouting:
                    _postRoutingMiddleware.Add(action);
                    break;
                case MiddlewareEntryPoint.PostResponse:
                    _postResponseMiddleware.Add(action);
                    break;
                case MiddlewareEntryPoint.Authentication:
                case MiddlewareEntryPoint.PreRouting:
                default:
                    throw new Exception();
                    break;
            }
        }

        /// <summary>
        /// starts the http server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="maxClients"></param>
        public void Start(IPAddress address, int port, int maxClients = 100)
        {
            // create listener
            _listener = new Griffin.Net.Protocols.Http.HttpListener(
                    new ChannelTcpListenerConfiguration(
                        () => bodyDecoder == null ? new HttpMessageDecoder() : new HttpMessageDecoder(bodyDecoder),
                        () => new HttpMessageEncoder()
                    )
                    {
                        BufferPool = new BufferSlicePool(65535, maxClients)
                    }
                );
            // Create body decoder
            var bdyDecoder = new CompositeIMessageSerializer();
            bdyDecoder.Add(MultipartSerializer.MimeType, new MultipartSerializer());
            bdyDecoder.Add(UrlFormattedMessageSerializer.MimeType, new UrlFormattedMessageSerializer());
            bodyDecoder = bdyDecoder;

            // client connected maybe create a list with connected channels
            _listener.ClientConnected += (object sender, ClientConnectedEventArgs e) =>
            {
                Interlocked.Increment(ref _connectedClients);
            };
            // client disconnected check why and decrement counter
            _listener.ClientDisconnected += (object sender, ClientDisconnectedEventArgs e) =>
            {
                if (e.Exception != null)
                {
                    var socketEx = e.Exception as SocketException;
                    if (socketEx != null)
                    {
                        if (socketEx.SocketErrorCode != SocketError.Success)
                        {
                            _logger.Error("HttpListener->ClientDisconnected", e.Exception);
                        }
                    }
                    else
                    {
                        _logger.Error("HttpListener->ClientDisconnected", e.Exception);
                    }
                }
                Interlocked.Decrement(ref _connectedClients);
            };
            // set message received handler
            _listener.MessageReceived = RecMessage;

            _router.MapAttributes();
            _listener.Start(address, port);
        }

        /// <summary>
        /// stops the listener
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
        }

        private void RecMessage(ITcpChannel channel, object message)
        {
            var request = (HttpRequest)message;
            HttpResponse response = null;

            // execute authentication middleware
            foreach (var middleware in _authMiddleware)
            {
                try
                {
                    var res = middleware(channel, request);
                    if (res != null)
                    {
                        response = res;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("WebServer->RecMessage->Execute authentication Middleware, {3}, Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query, middleware.ToString()), ex);
                }
            }
            if (response == null)
            {
                // execute prerouting middleware
                foreach (var middleware in _preRoutingMiddleware)
                {
                    try
                    {
                        var res = middleware(channel, request);
                        if (res != null)
                        {
                            response = res;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("WebServer->RecMessage->Execute pre routing Middleware, {3}, Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query, middleware.ToString()), ex);
                    }
                }
            }

            try
            {
                if (response == null)
                {
                    // execute router
                    var res = _router.Route(request);
                    if (res != null)
                    {
                        var resResponse = res as HttpResponse;
                        if (resResponse != null)
                        {
                            response = resResponse;
                        }
                        else
                        {
                            // to reduce dependencies on libraries remove this
                            throw new Exception("Routes must return HttpResponse's");
                            // response was no HttpResponse so just return it as json
                            //response = request.CreateResponse();
                            //response.StatusCode = (int)HttpStatusCode.OK;
                            //response.ContentType = "application/json";
                            //response.Body = new MemoryStream(response.ContentCharset.GetBytes(JsonConvert.SerializeObject(res)));
                        }
                    }
                }
            }
            catch(UnauthorizedAccessException) {
                // router or controller throws UnauthorizedAccess so return 403
                if (On403 == null)
                    response = Default403(request);
                else
                    response = On403(request);
            }
            catch(AuthenticationException) {
                // router or controller throws Authentication so return 401
                if (On401 == null)
                    response = Default401(request);
                else
                    response = On401(request);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("WebServer->RecMessage->ProcessRequest Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query), ex);
            }

            try
            {
                // looks like 404 create one
                if (response == null && On404 != null)
                    response = On404(request);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("WebServer->RecMessage->On404 Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query), ex);
            }

            if (response == null)
                response = Default404(request);

            // execute post routing middleware
            foreach (var middleware in _postRoutingMiddleware)
            {
                try
                {
                    middleware(channel, request, response);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("WebServer->RecMessage->Execute post routing Middleware, {3}, Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query, middleware.ToString()), ex);
                }
            }

            // send response
            channel.Send(response);

            // when the HttpVersion is 1.0 close the connection after sending
            if (request.HttpVersion == "HTTP/1.0")
                channel.Close();

            // execute post response middleware
            // dont know where you need it expect for statistics
            foreach (var middleware in _postResponseMiddleware)
            {
                try
                {
                    middleware(channel, request, response);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("WebServer->RecMessage->Execute post response Middleware, {3}, Path: {0} From: {1} Query: {2}", request.Uri.AbsolutePath, request.RemoteEndPoint.ToString(), request.Uri.Query, middleware.ToString()), ex);
                }
            }
        }

        private HttpResponse Default404(HttpRequest request)
        {
            // create default 404
            var resp = request.CreateResponse();
            resp.StatusCode = (int)HttpStatusCode.NotFound;
            return resp;
        }
        private HttpResponse Default401(HttpRequest request)
        {
            // create default 401
            var resp = request.CreateResponse();
            resp.StatusCode = (int)HttpStatusCode.Unauthorized;
            return resp;
        }
        private HttpResponse Default403(HttpRequest request)
        {
            // create default 403
            var resp = request.CreateResponse();
            resp.StatusCode = (int)HttpStatusCode.Forbidden;
            return resp;
        }

        #region IDisposable Member
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Griffin.RoutedWebServer"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="Griffin.RoutedWebServer"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="Griffin.RoutedWebServer"/> so
        /// the garbage collector can reclaim the memory that the <see cref="Griffin.RoutedWebServer"/> was occupying.</remarks>
        public void Dispose()
        {
            if (_listener != null)
                Stop();

            _listener = null;
        }
        #endregion
    }
}
