using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http
{
    public class HttpServer
    {
        private readonly HttpConfiguration _configuration;
        private MessagingServer<HttpContext> _server;
        private MessagingServer<HttpContext> _secureServer;
        private readonly BufferManager _bufferManager = new BufferManager(10);

        public HttpServer(HttpConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Port assigned to the http server (when 0 was used to let the OS specify the port)
        /// </summary>
        public int LocalPort { get; private set; }
        /// <summary>
        /// Port assigned to the https server (when 0 was used to let the OS specify the port)
        /// </summary>
        public int SecurePort { get; private set; }

        public async Task RunAsync(IPAddress address, CancellationToken token)
        {
            if (_configuration.Pipeline.Count == 0)
                throw new InvalidOperationException("No middleware have been registered. Cannot run HTTP server.");

            var tasks = new List<Task>();
            
            if (_configuration.Certificate != null)
            {
                var secureConfig = new MessagingServerConfiguration<HttpContext>
                {
                    HandlerFactory = CreateSecureHandler,
                    BufferPool = _bufferManager
                };
                _secureServer = new MessagingServer<HttpContext>(secureConfig);

                var task = _secureServer.RunAsync(address, _configuration.SecurePort, token);
                tasks.Add(task);
                SecurePort = _secureServer.LocalPort;
            }

            if (_configuration.Port != -1)
            {
                var config = new MessagingServerConfiguration<HttpContext>
                {
                    HandlerFactory = CreateHandler,
                    BufferPool = _bufferManager
                };
                _server = new MessagingServer<HttpContext>(config);
                var task = _server.RunAsync(address, _configuration.Port, token);
                tasks.Add(task);
                LocalPort = _server.LocalPort;

            }

            await Task.WhenAll(tasks);
        }

        private IClientHandler<HttpContext> CreateHandler(MessagingServerHandlerFactoryContext arg)
        {
            var handler= new HttpHandler(arg.Socket, _bufferManager.Dequeue(), _configuration.Pipeline,_configuration.ContentSerializers);
            return handler;
        }

        private IClientHandler<HttpContext> CreateSecureHandler(MessagingServerHandlerFactoryContext arg)
        {
            var sslBuilder=  new ServerSideSslStreamBuilder(_configuration.Certificate);
            var secureChannel = new SecureTcpChannel(sslBuilder);
            secureChannel.Assign(arg.Socket);
            return new HttpHandler(secureChannel, _bufferManager.Dequeue(), _configuration.Pipeline, _configuration.ContentSerializers);
        }

        public async Task StopAsync()
        {
            if (_server != null)
                await _server.StopAsync();
            if (_secureServer != null)
                await _secureServer.StopAsync();
        }
    }
}