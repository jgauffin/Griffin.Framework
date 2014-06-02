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

        public ChannelTcpClient(IMessageEncoder encoder, IMessageDecoder decoder)
            : this(encoder, decoder, new BufferSlice(new byte[65535], 0, 65535))
        {
        }

        public ChannelTcpClient(IMessageEncoder encoder, IMessageDecoder decoder, BufferSlice readBuffer)
        {
            if (encoder == null) throw new ArgumentNullException("encoder");
            if (decoder == null) throw new ArgumentNullException("decoder");
            _channel = new TcpChannel(readBuffer, encoder, decoder)
            {
                Disconnected = OnDisconnect,
                MessageSent = OnSendCompleted,
                MessageReceived = OnChannelMessageReceived
            };

            decoder.MessageReceived = OnMessageReceived;
            _args.Completed += OnConnect;
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

        public async Task CloseAsync()
        {
            await _channel.CloseAsync();
            _channel = null;
        }

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


        public Task<T> ReceiveAsync()
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        public Task<T> ReceiveAsync(CancellationToken cancellation)
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        public Task<T> ReceiveAsync(TimeSpan timeout)
        {
            return ReceiveAsync(timeout, CancellationToken.None);
        }


        public async Task<T> ReceiveAsync(TimeSpan timeout, CancellationToken cancellation)
        {
            await _readSemaphore.WaitAsync(timeout, cancellation);
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
            _readItems.Enqueue((T) message);
            _readSemaphore.Release();
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _connectException = new SocketException((int) e.SocketError);
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

    public class ChannelException : Exception
    {
        public ChannelException(string errorMessage, Exception inner)
            : base(errorMessage, inner)
        {
        }

        public ChannelException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}