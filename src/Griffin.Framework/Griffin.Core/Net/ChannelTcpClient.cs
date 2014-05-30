using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;

namespace Griffin.Net
{
    /// <summary>
    /// Can talk with messaging servers (i.e. servers based on <see cref="ChannelTcpListener"/>).
    /// </summary>
    /// <typeparam name="T">Type of message to receive</typeparam>
    public class ChannelTcpClient<T> : IDisposable
    {
        private readonly SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
        private TcpChannel _channel;
        private TaskCompletionSource<IPEndPoint> _connectCompletionSource;
        private TaskCompletionSource<T> _readCompletionSource;
        private TaskCompletionSource<T> _sendCompletionSource;
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

        public Task ConnectAsync(IPAddress address, int port)
        {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");
            if (_connectCompletionSource != null)
                throw new InvalidOperationException("There is already a pending connect.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _args.RemoteEndPoint = new IPEndPoint(address, port);
            var isPending = _socket.ConnectAsync(_args);

            _connectCompletionSource = new TaskCompletionSource<IPEndPoint>();
            if (!isPending)
            {
                _connectCompletionSource.SetResult((IPEndPoint) _socket.RemoteEndPoint);
            }

            return _connectCompletionSource.Task;
        }


        public Task<T> ReceiveAsync()
        {
            if (_readCompletionSource != null)
                throw new InvalidOperationException("There is already a pending receive operation.");

            _readCompletionSource = new TaskCompletionSource<T>();
            _readCompletionSource.Task.ConfigureAwait(false);

            return _readCompletionSource.Task;
        }

        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (_sendCompletionSource != null)
                throw new InvalidOperationException("There is already a pending send operation.");

            _sendCompletionSource = new TaskCompletionSource<T>();
            _sendCompletionSource.Task.ConfigureAwait(false);

            _channel.Send(message);
            return _sendCompletionSource.Task;
        }

        private void OnChannelMessageReceived(ITcpChannel channel, object message)
        {
            _readCompletionSource.SetResult((T) message);
            _readCompletionSource = null;
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _connectCompletionSource.SetException(new SocketException((int) e.SocketError));
                _connectCompletionSource = null;
                return;
            }

            _channel.Assign(_socket);
            _connectCompletionSource.SetResult((IPEndPoint) _socket.RemoteEndPoint);
            _connectCompletionSource = null;
        }

        private void OnDisconnect(ITcpChannel arg1, Exception arg2)
        {
            _socket = null;

            if (_sendCompletionSource != null)
            {
                _sendCompletionSource.SetException(arg2);
                _sendCompletionSource = null;
            }

            if (_readCompletionSource != null)
            {
                _readCompletionSource.SetException(arg2);
                _readCompletionSource = null;
            }
        }

        private void OnMessageReceived(object obj)
        {
            _readCompletionSource.SetResult((T) obj);
        }

        private void OnSendCompleted(ITcpChannel channel, object sentMessage)
        {
            _sendCompletionSource.SetResult((T) sentMessage);
            _sendCompletionSource = null;
        }

        public async Task CloseAsync()
        {
            await _channel.CloseAsync();
            _channel = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_channel == null)
                return;

            _channel.Close();
            _channel = null;
        }
    }
}