using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Net
{
    /// <summary>
    ///     Can talk with messaging servers (i.e. servers based on <see cref="ChannelTcpListener" />).
    /// </summary>
    /// <typeparam name="T">Type of message to receive</typeparam>
    public class ChannelTcpClient<T> : IDisposable
    {
        private readonly SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
        private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(0, 1);
        private readonly ConcurrentQueue<object> _readItems = new ConcurrentQueue<object>();
        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _sendCompletedSemaphore = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _sendQueueSemaphore = new SemaphoreSlim(1, 1);
        private TcpChannel _channel;
        private Exception _connectException;
        private Exception _sendException;
        private Socket _socket;
        private FilterMessageHandler _filterHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelTcpClient{T}"/> class.
        /// </summary>
        /// <param name="encoder">Used to encode outbound messages.</param>
        /// <param name="decoder">Used to decode inbound messages.</param>
        public ChannelTcpClient(IMessageEncoder encoder, IMessageDecoder decoder)
            : this(encoder, decoder, new BufferSlice(new byte[65535], 0, 65535))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelTcpClient{T}" /> class.
        /// </summary>
        /// <param name="encoder">Used to encode outbound messages.</param>
        /// <param name="decoder">Used to decode inbound messages.</param>
        /// <param name="readBuffer">Buffer used to receive bytes..</param>
        /// <exception cref="System.ArgumentNullException">
        /// encoder
        /// or
        /// decoder
        /// or
        /// readBuffer
        /// </exception>
        public ChannelTcpClient(IMessageEncoder encoder, IMessageDecoder decoder, IBufferSlice readBuffer)
        {
            if (encoder == null) throw new ArgumentNullException("encoder");
            if (decoder == null) throw new ArgumentNullException("decoder");
            if (readBuffer == null) throw new ArgumentNullException("readBuffer");

            _channel = new TcpChannel(readBuffer, encoder, decoder)
            {
                Disconnected = OnDisconnect,
                MessageSent = OnSendCompleted,
                MessageReceived = OnChannelMessageReceived
            };

            decoder.MessageReceived = OnMessageReceived;
            _args.Completed += OnConnect;
        }

        public bool Connected
        {
            get
            {
                return _
            }
        }
        /// <summary>
        /// Delegate which can be used instead of <see cref="ReceiveAsync()"/> or to inspect all incoming messages before they are passed to <see cref="ReceiveAsync()"/>.
        /// </summary>
        public FilterMessageHandler Filter
        {
            get { return _filterHandler; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _filterHandler = value;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_channel == null)
                return;

            _channel.Close();
            _channel = null;
        }

        /// <summary>
        /// Wait for all messages to be sent and close the connection
        /// </summary>
        /// <returns>Async task</returns>
        public async Task CloseAsync()
        {
            await _channel.CloseAsync();
            _channel = null;
        }

        /// <summary>
        /// Connects to remote end point.
        /// </summary>
        /// <param name="address">Address to connect to.</param>
        /// <param name="port">Remote port.</param>
        /// <returns>Async task</returns>
        /// <exception cref="System.InvalidOperationException">Socket is already connected</exception>
        public async Task ConnectAsync(IPAddress address, int port)
        {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _args.RemoteEndPoint = new IPEndPoint(address, port);
            var isPending = _socket.ConnectAsync(_args);

            if (!isPending)
                return;

            await _connectSemaphore.WaitAsync();
            if (_connectException != null)
                throw _connectException;
        }

        /// <summary>
        /// Connects to remote end point.
        /// </summary>
        /// <param name="address">Address to connect to.</param>
        /// <param name="port">Remote port.</param>
        /// <param name="timeout">Maximum amount of time to wait for a connection.</param>
        /// <returns>Async task</returns>
        /// <exception cref="System.InvalidOperationException">Socket is already connected</exception>
        public async Task ConnectAsync(IPAddress address, int port, TimeSpan timeout)
        {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _args.RemoteEndPoint = new IPEndPoint(address, port);
            var isPending = _socket.ConnectAsync(_args);

            if (!isPending)
                return;

            await _connectSemaphore.WaitAsync(timeout);
        }


        /// <summary>
        /// Receive a message
        /// </summary>
        /// <returns>Decoded message</returns>
        public Task<T> ReceiveAsync()
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        /// <summary>
        /// Receive a message
        /// </summary>
        /// <param name="cancellation">Token used to cancel the pending read operation.</param>
        /// <returns>
        /// Decoded message if successful; <c>default(T)</c> if cancellation is requested.
        /// </returns>
        public Task<T> ReceiveAsync(CancellationToken cancellation)
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <param name="timeout">Maximum amount of time to wait on a message</param>
        /// <returns>Decoded message</returns>
        /// <exception cref="Griffin.Net.ChannelException">Was signalled that something have been recieved, but found nothing in the in queue</exception>
        public Task<T> ReceiveAsync(TimeSpan timeout)
        {
            return ReceiveAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        /// Receive a message
        /// </summary>
        /// <param name="timeout">Maximum amount of time to wait on a message</param>
        /// <param name="cancellation">Token used to cancel the pending read operation.</param>
        /// <returns>
        /// Decoded message if successful; <c>default(T)</c> if cancellation is requested.
        /// </returns>
        /// <exception cref="Griffin.Net.ChannelException">Was signalled that something have been recieved, but found nothing in the in queue</exception>
        public async Task<T> ReceiveAsync(TimeSpan timeout, CancellationToken cancellation)
        {
            await _readSemaphore.WaitAsync(timeout, cancellation);
            if (cancellation.IsCancellationRequested)
                return default(T);

            object item;
            var gotItem = _readItems.TryDequeue(out item);

            if (!gotItem)
                throw new ChannelException(
                    "Was signalled that something have been recieved, but found nothing in the in queue");

            if (item is ChannelException)
                throw (ChannelException) item;

            // signal so that more items can be read directly
            if (_readItems.Count > 0)
                _readSemaphore.Release();

            return (T) item;
        }

        /// <summary>
        /// Send message to the remote end point.
        /// </summary>
        /// <param name="message">message to send.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <remarks>
        /// <para>
        /// All messages are being enqueued and sent in order. This method will return when the current message have been sent. It
        /// </para>
        /// <para>
        /// The method is thread safe and can be executed from multiple threads.
        /// </para>
        /// </remarks>
        public async Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException("message");


            await _sendQueueSemaphore.WaitAsync();

            _channel.Send(message);


            await _sendCompletedSemaphore.WaitAsync();
            _sendQueueSemaphore.Release();
        }

        private void OnChannelMessageReceived(ITcpChannel channel, object message)
        {
            if (_filterHandler != null)
            {
                var result = _filterHandler(channel, message);
                if (result == ClientFilterResult.Revoke)
                    return;
            }
                

            _readItems.Enqueue((T) message);
            _readSemaphore.Release();
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _connectException = new SocketException((int) e.SocketError);
                _socket = null;
            }

            _channel.Assign(_socket);
            _connectSemaphore.Release();
        }

        private void OnDisconnect(ITcpChannel arg1, Exception arg2)
        {
            _socket = null;

            if (_sendCompletedSemaphore.CurrentCount == 0)
            {
                _sendException = arg2;
                _sendCompletedSemaphore.Release();
            }

            if (_readSemaphore.CurrentCount == 0)
            {
                _readItems.Enqueue(new ChannelException("Socket got disconnected", arg2));
                _readSemaphore.Release();
            }
        }

        private void OnMessageReceived(object obj)
        {
            _readItems.Enqueue(obj);
            if (_readSemaphore.CurrentCount == 0)
                _readSemaphore.Release();
        }

        private void OnSendCompleted(ITcpChannel channel, object sentMessage)
        {
            _sendCompletedSemaphore.Release();
        }
    }
}