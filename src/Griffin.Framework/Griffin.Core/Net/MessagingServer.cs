using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net
{
    /// <summary>
    ///     Listens on one of the specified protocols
    /// </summary>
    public class MessagingServer<TContext> where TContext : IMiddlewareContext
    {
        private BufferManager _bufferPool;
        private CancellationToken _cancellationToken;
        private MessagingServerConfiguration<TContext> _configuration;
        private bool _shuttingDown = false;
        private readonly LinkedList<IClientHandler<TContext>>
            _freehandlers = new LinkedList<IClientHandler<TContext>>();

        private TcpListener _listener;
        private MessagingServerPipeline<TContext> _pipeline;

        private readonly LinkedList<IClientHandler<TContext>>
            _usedHandlers = new LinkedList<IClientHandler<TContext>>();

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public MessagingServer(MessagingServerConfiguration<TContext> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            Configure(configuration);
        }

        /// <summary>
        ///     Port that the server is listening on.
        /// </summary>
        /// <remarks>
        ///     You can use port <c>0</c> in <see cref="MessagingServerConfiguration{TContext}" /> to let the OS assign a port.
        ///     This method will then give you the
        ///     assigned port.
        /// </remarks>
        public int LocalPort
        {
            get
            {
                if (_listener == null)
                    return -1;

                return ((IPEndPoint) _listener.LocalEndpoint).Port;
            }
        }

        /// <summary>
        ///     An internal error occurred
        /// </summary>
        public event EventHandler<ErrorEventArgs> ListenerError;

        /// <summary>
        ///     Start this listener.
        /// </summary>
        /// <remarks>
        ///     This also pre-configures 20 channels that can be used and reused during the lifetime of
        ///     this listener.
        /// </remarks>
        /// <param name="address">Address to accept connections on</param>
        /// <param name="port">Port to use. Set to <c>0</c> to let the OS decide which port to use. </param>
        /// <seealso cref="LocalPort" />
        public virtual async Task RunAsync(IPAddress address, int port, CancellationToken cancellationToken)
        {
            if (port < 0)
                throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be 0 or more.");
            if (_listener != null)
                throw new InvalidOperationException("Already listening.");

            _cancellationToken = cancellationToken;
            _listener = new TcpListener(address, port);
            _listener.Start();

            cancellationToken.Register(() =>
            {
                _shuttingDown = true;
                StopAsync().GetAwaiter().GetResult();
            });
            await AcceptSockets();
        }

        /// <summary>
        ///     Stop the listener.
        /// </summary>
        public virtual async Task StopAsync()
        {
            Debug.Print("Stopping...");
            _shuttingDown = true;
            _listener.Stop();
            var tasks = _usedHandlers
                .Select(x => x.CloseAsync())
                .ToArray();
            if (tasks.Any())
                await Task.WhenAll(tasks);
        }

        /// <summary>
        ///     To allow the sub classes to configure this class in their constructors.
        /// </summary>
        /// <param name="configuration"></param>
        protected void Configure(MessagingServerConfiguration<TContext> configuration)
        {
            _bufferPool = configuration.BufferPool;
            _configuration = configuration;
        }

        /// <summary>
        ///     Invoke when the listener itself fails
        /// </summary>
        /// <param name="ex">Caught exception</param>
        protected virtual void OnListenerError(Exception ex)
        {
            ListenerError?.Invoke(this, new ErrorEventArgs(ex));
        }

        private async Task AcceptSockets()
        {
            while (!_cancellationToken.IsCancellationRequested && !_shuttingDown)
            {
                try
                {
                    Debug.Print("Accepting...");
                    var socket = await _listener.AcceptSocketAsync();
                    if (socket == null)
                    {
                        continue;
                    }

                    var factoryContext = new MessagingServerHandlerFactoryContext
                    {
                        EndPoint = socket.RemoteEndPoint,
                        Socket = socket
                    };
                    var client = _configuration.HandlerFactory(factoryContext);

#pragma warning disable 4014
                    client.ProcessAsync(_pipeline)
                        .ContinueWith(async x =>
#pragma warning restore 4014
                        {
                            await client.CloseAsync();
                            client.Reset();
                        }, _cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    //thrown when we call listener.Stop();
                    // can't seem to be able to shutdown gracefully.
                    break;
                }
                catch (Exception exception)
                {
                    OnListenerError(exception);
                    await Task.Delay(100, _cancellationToken);
                }
            }
        }

        private void ResetClient(IClientHandler<TContext> handler)
        {
            _usedHandlers.Remove(handler);
            handler.Reset();
            _freehandlers.AddLast(handler);
        }

        private async Task SendStream(IBinaryChannel channel, Stream response)
        {
            var buf = new byte[response.Length];
            await response.ReadAsync(buf, 0, buf.Length);
            await channel.SendAsync(buf, 0, buf.Length);
        }
    }
}