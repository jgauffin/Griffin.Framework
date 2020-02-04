using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Authentication;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Messaging;
using Griffin.Net.Protocols;

namespace Griffin.Net
{
    public class MessagingClient : IMessagingChannel
    {
        private readonly IMessageDecoder _decoder;
        private readonly IMessageEncoder _encoder;
        private readonly IBufferSegment _readBuffer;
        private IBinaryChannel _channel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagingClient" /> class.
        /// </summary>
        /// <param name="encoder">Used to encode outbound messages.</param>
        /// <param name="decoder">Used to decode inbound messages.</param>
        public MessagingClient(IMessageEncoder encoder, IMessageDecoder decoder)
            : this(encoder, decoder, new StandAloneBuffer(new byte[65535], 0, 65535))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagingClient" /> class.
        /// </summary>
        /// <param name="encoder">Used to encode outbound messages.</param>
        /// <param name="decoder">Used to decode inbound messages.</param>
        /// <param name="readBuffer">Buffer used to receive bytes..</param>
        /// <exception cref="System.ArgumentNullException">
        ///     encoder
        ///     or
        ///     decoder
        ///     or
        ///     readBuffer
        /// </exception>
        public MessagingClient(IMessageEncoder encoder, IMessageDecoder decoder, IBufferSegment readBuffer)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            _readBuffer = readBuffer ?? throw new ArgumentNullException(nameof(readBuffer));
        }

        /// <summary>
        ///     Set if you want to authenticate against a server.
        /// </summary>
        public IClientAuthenticator Authenticator { get; set; }

        /// <summary>
        ///     Set certificate if you want to use secure connections.
        /// </summary>
        public ISslStreamBuilder Certificate { get; set; }

        /// <summary>
        ///     Gets if channel is connected
        /// </summary>
        public bool IsConnected => _channel != null && _channel.IsConnected;

        /// <summary>
        ///     Receive a message
        /// </summary>
        /// <returns>Decoded message</returns>
        public Task<object> ReceiveAsync()
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        /// <summary>
        ///     Receive a message
        /// </summary>
        /// <param name="cancellation">Token used to cancel the pending read operation.</param>
        /// <returns>
        ///     Decoded message if successful; <c>default(T)</c> if cancellation is requested.
        /// </returns>
        public Task<object> ReceiveAsync(CancellationToken cancellation)
        {
            return ReceiveAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
        }

        /// <summary>
        ///     Receives the asynchronous.
        /// </summary>
        /// <param name="timeout">Maximum amount of time to wait on a message</param>
        /// <returns>Decoded message</returns>
        /// <exception cref="Griffin.Net.ChannelException">
        ///     Was signaled that something have been received, but found nothing in
        ///     the in queue
        /// </exception>
        public Task<object> ReceiveAsync(TimeSpan timeout)
        {
            return ReceiveAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        ///     Send message to the remote end point.
        /// </summary>
        /// <param name="message">message to send.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <remarks>
        ///     <para>
        ///         All messages are being queued and sent in order. This method will return when the current message have been
        ///         sent. It
        ///     </para>
        ///     <para>
        ///         The method is thread safe and can be executed from multiple threads.
        ///     </para>
        /// </remarks>
        public async Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException("message");

            await _encoder.EncodeAsync(message, _channel);
        }

        public void Assign(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            EnsureChannel();
            _channel.Assign(socket);
        }

        /// <summary>
        ///     Wait for all messages to be sent and close the connection
        /// </summary>
        /// <returns>Async task</returns>
        public async Task CloseAsync()
        {
            await _channel.CloseAsync();
            _channel = null;
        }

        /// <summary>
        ///     Connects to remote end point.
        /// </summary>
        /// <param name="address">Address to connect to.</param>
        /// <param name="port">Remote port.</param>
        /// <returns>Async task</returns>
        /// <exception cref="System.InvalidOperationException">Socket is already connected</exception>
        public async Task ConnectAsync(IPAddress address, int port)
        {
            if (_channel != null)
                throw new InvalidOperationException("Socket is already connected");

            _channel = new TcpChannel();
            var ep = new IPEndPoint(address, port);
            await _channel.OpenAsync(ep);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_channel == null)
                return;

            _channel.CloseAsync().GetAwaiter().GetResult();
            _channel = null;
        }

        /// <summary>
        ///     Receive a message
        /// </summary>
        /// <param name="timeout">Maximum amount of time to wait on a message</param>
        /// <param name="cancellation">Token used to cancel the pending read operation.</param>
        /// <returns>
        ///     Decoded message if successful; <c>default(T)</c> if cancellation is requested.
        /// </returns>
        /// <exception cref="Griffin.Net.ChannelException">
        ///     Was signaled that something have been received, but found nothing in
        ///     the in queue
        /// </exception>
        public async Task<object> ReceiveAsync(TimeSpan timeout, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested)
                return default;

            while (true)
            {
                var msg = await _decoder.DecodeAsync(_channel, _readBuffer);
                return msg;
            }
        }

        private void EnsureChannel()
        {
            if (_channel != null)
                return;

            if (Certificate != null)
                _channel = new SecureTcpChannel(Certificate);
            else
                _channel = new TcpChannel();
        }
    }
}