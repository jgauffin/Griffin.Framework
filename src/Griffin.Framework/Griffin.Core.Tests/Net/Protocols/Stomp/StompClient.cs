using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Core.Tests.Net.Protocols.Stomp
{

    /// <summary>
    /// A client implementation for transferring objects over the MicroMessage protocol
    /// </summary>
    public class MicroMessageClient
    {
        private MicroMessageDecoder _decoder;
        private MicroMessageEncoder _encoder;
        private TcpChannel _channel;
        private TaskCompletionSource<IPEndPoint> _connectCompletionSource;
        private TaskCompletionSource<object> _readCompletionSource;
        private TaskCompletionSource<object> _sendCompletionSource;
        private Socket _socket;
        private SocketAsyncEventArgs _args = new SocketAsyncEventArgs();

        public MicroMessageClient(IMessageSerializer serializer)
        {
            _decoder = new MicroMessageDecoder(serializer);
            _encoder = new MicroMessageEncoder(serializer);
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), _encoder, _decoder);
            _channel.Disconnected = OnDisconnect;
            _channel.MessageSent = OnSendCompleted;
            _channel.MessageReceived = OnChannelMessageReceived;
            _decoder.MessageReceived = OnMessageReceived;
            _args.Completed += OnConnect;
        }

        private void OnChannelMessageReceived(ITcpChannel channel, object message)
        {
            _readCompletionSource.SetResult(message);
            _readCompletionSource = null;
        }

        private void OnSendCompleted(ITcpChannel channel, object sentMessage)
        {
            _sendCompletionSource.SetResult(sentMessage);
            _sendCompletionSource = null;
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

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _connectCompletionSource.SetException(new SocketException((int)e.SocketError));
                _connectCompletionSource = null;
                return;
            }

            _channel.Assign(_socket);
            _connectCompletionSource.SetResult((IPEndPoint)_socket.RemoteEndPoint);
            _connectCompletionSource = null;
        }

        public Task ConnectAsync(IPAddress address, int port)
        {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");
            if (_connectCompletionSource != null)
                throw new InvalidOperationException("There is already a pending connect.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _args.RemoteEndPoint = new IPEndPoint(address, port);
            var isPending=_socket.ConnectAsync(_args);

            _connectCompletionSource = new TaskCompletionSource<IPEndPoint>();
            if (!isPending)
            {
                _connectCompletionSource.SetResult((IPEndPoint)_socket.RemoteEndPoint);
            }

            return _connectCompletionSource.Task;
        }


        private void OnMessageReceived(object obj)
        {
            _readCompletionSource.SetResult(obj);
        }

        public Task<Object> ReceiveAsync()
        {
            if (_readCompletionSource != null)
                throw new InvalidOperationException("There is already a pending receive operation.");

            _readCompletionSource = new TaskCompletionSource<object>();
            _readCompletionSource.Task.ConfigureAwait(false);

            return _readCompletionSource.Task;
        }

        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (_sendCompletionSource != null)
                throw new InvalidOperationException("There is already a pending send operation.");

            _sendCompletionSource = new TaskCompletionSource<object>();
            _sendCompletionSource.Task.ConfigureAwait(false);

            _channel.Send(message);
            return _sendCompletionSource.Task;
        }
    }
}
