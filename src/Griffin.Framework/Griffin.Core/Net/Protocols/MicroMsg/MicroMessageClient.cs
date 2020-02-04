using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Messaging;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    ///     A client implementation for transferring objects over the MicroMessage protocol
    /// </summary>
    public class MicroMessageClient:IMessagingChannel
    {
        private readonly MicroMessageDecoder _decoder;
        private readonly MicroMessageEncoder _encoder;
        private readonly ClientSideSslStreamBuilder _sslStreamBuilder;
        private IBinaryChannel _channel;
        private IBufferSegment _readBuffer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageClient" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public MicroMessageClient(IMessageSerializer serializer, BufferManager bufferManager)
        {
            _decoder = new MicroMessageDecoder(serializer);
            _encoder = new MicroMessageEncoder(serializer);
            _readBuffer = bufferManager.Dequeue();
            _channel = new TcpChannel();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageClient" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="sslStreamBuilder">The SSL stream builder.</param>
        public MicroMessageClient(IMessageSerializer serializer, ClientSideSslStreamBuilder sslStreamBuilder)
        {
            _sslStreamBuilder = sslStreamBuilder;
            _decoder = new MicroMessageDecoder(serializer);
            _encoder = new MicroMessageEncoder(serializer);
            _channel = new SecureTcpChannel(_sslStreamBuilder);

        }

        public async Task ConnectAsync()
        {
            await _channel.OpenAsync();
        }

        public bool IsConnected => _channel.IsConnected;

        private IBinaryChannel CreateChannel()
        {
            if (_channel != null)
                return _channel;

            return _channel;
        }

        public Task<object> ReceiveAsync(CancellationToken token)
        {
            return _decoder.DecodeAsync(_channel, _readBuffer, token);
        }

        public Task<object> ReceiveAsync(TimeSpan timeout)
        {
            return _decoder.DecodeAsync(_channel, _readBuffer, timeout);
        }

        /// <summary>
        ///     Receive an object
        /// </summary>
        /// <returns>completion task</returns>
        public async Task<object> ReceiveAsync()
        {
            return await _decoder.DecodeAsync(_channel, _readBuffer);
        }

        /// <summary>
        ///     Send an object
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>completion task (completed once the message have been delivered).</returns>
        public async Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await _encoder.EncodeAsync(message, _channel);
        }

        public async Task ConnectAsync(IPAddress address, int port)
        {
            await _channel.OpenAsync(new IPEndPoint(address, port));
        }
    }
}