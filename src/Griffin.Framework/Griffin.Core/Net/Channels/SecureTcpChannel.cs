using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Griffin.Net.Buffers;

namespace Griffin.Net.Channels
{
    public class SecureTcpChannel : IBinaryChannel
    {
        private readonly List<IPooledObject> _pooledWriteBuffers = new List<IPooledObject>();
        private readonly List<IBufferSegment> _queuedOutboundPackets = new List<IBufferSegment>();
        private SslStream _ioStream;
        private Socket _socket;
        private readonly ISslStreamBuilder _sslStreamBuilder;
        private volatile ChannelState _state;

        public SecureTcpChannel(ISslStreamBuilder sslStreamBuilder)
        {
            _sslStreamBuilder = sslStreamBuilder ?? throw new ArgumentNullException(nameof(sslStreamBuilder));
            MaxBytesPerWriteOperation = 65535;
            ChannelData = new ChannelData();
            RemoteEndpoint = EmptyEndpoint.Instance;
        }

        /// <summary>
        /// </summary>
        internal ChannelState State => _state;

        /// <summary>
        ///     Address to the end point that we should connect to (or address to the EP if this is a server side channel).
        /// </summary>
        /// <remarks>
        ///     <para>Typically a <see cref="IPEndPoint" /> or <see cref="DnsEndPoint" />.</para>
        /// </remarks>
        public EndPoint RemoteEndpoint { get; private set; }

        /// <summary>
        ///     Indicates if the channel is open or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The reliability of this flag varies depending on the communication technology. The connection
        ///         might be down due to network failures etc. hence it tells what the channel sees as the current truth,
        ///         but that can change with the next IO operation which would challenge that truth.
        ///     </para>
        /// </remarks>
        public bool IsOpen => State == ChannelState.Open;

        /// <summary>
        ///     You can use channel data to store connection specific information (compare it to a HTTP session).
        /// </summary>
        public IChannelData ChannelData { get; }

        /// <summary>
        ///     Channel data is an dictionary which means a lookup every time. This token can be used as an alternative.
        /// </summary>
        public object UserToken { get; set; }

        public bool IsConnected => _socket?.Connected == true;

        /// <summary>
        ///     Amount of bytes which can be included in a send operation.
        /// </summary>
        public int MaxBytesPerWriteOperation { get; set; }

        /// <summary>
        ///     Channel have been closed (by either side).
        /// </summary>
        public event EventHandler ChannelClosed;

        public Task CloseAsync()
        {
            _ioStream.Close();
            _socket.Close();
            _ioStream = null;
            _socket = null;
            ChannelClosed?.Invoke(this, EventArgs.Empty);
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Assign a socket to this channel.
        /// </summary>
        /// <param name="socket">A connected socket.</param>
        /// <exception cref="InvalidOperationException">
        ///     This channel already have an connected socket. Cleanup before assigning a
        ///     new one.
        /// </exception>
        public void Assign(Socket socket)
        {
            if (_socket?.Connected == true)
                throw new InvalidOperationException(
                    "This channel already have an connected socket. Cleanup before assigning a new one.");

            _socket = socket;
            _ioStream = _sslStreamBuilder.BuildAsync(this, _socket).GetAwaiter().GetResult();
        }

        public async Task OpenAsync()
        {
            if (RemoteEndpoint == EmptyEndpoint.Instance)
                throw new InvalidOperationException("No remote endpoint have been specified.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var awaitable = new SocketAwaitable(new SocketAsyncEventArgs());
            await awaitable.ConnectAsync(_socket, RemoteEndpoint);
            RemoteEndpoint = _socket.RemoteEndPoint;
            _ioStream = await _sslStreamBuilder.BuildAsync(this, _socket);
        }

        public async Task OpenAsync(EndPoint endpoint)
        {
            RemoteEndpoint = endpoint;
            await OpenAsync();
        }

        void IBinaryChannel.Reset()
        {
            Reset();
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            if (!_queuedOutboundPackets.Any())
            {
                await _ioStream.WriteAsync(buffer, offset, count);
                return;
            }

            _queuedOutboundPackets.Add(new StandAloneBuffer(buffer, offset, count));
            await SendAllQueuedBuffers();
        }

        public async Task SendAsync(IEnumerable<IBufferSegment> buffers)
        {
            await SendAllQueuedBuffers();

            foreach (var packet in buffers) await _ioStream.WriteAsync(packet.Buffer, packet.Offset, packet.Count);
        }

        public async Task SendAsync(IBufferSegment buffer)
        {
            await SendAllQueuedBuffers();
            await _ioStream.WriteAsync(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public async Task SendMoreAsync(byte[] buffer, int offset, int count)
        {
            _queuedOutboundPackets.Add(new StandAloneBuffer(buffer, offset, count));
            if (_queuedOutboundPackets.Sum(x => x.Count) < MaxBytesPerWriteOperation)
                return;

            await SendAllQueuedBuffers();
        }

        public async Task SendMoreAsync(IBufferSegment segment)
        {
            _queuedOutboundPackets.Add(segment);

            if (segment is IPooledObject p)
                _pooledWriteBuffers.Add(p);

            if (_queuedOutboundPackets.Sum(x => x.Count) < MaxBytesPerWriteOperation)
                return;

            await SendAllQueuedBuffers();
        }

        /// <summary>
        ///     Receive bytes from the remote endpoint.
        /// </summary>
        /// <param name="readBuffer">Buffer to fill with data.</param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         This method will receive from the current offset and up to the amount of bytes that are left from the current
        ///         offset to the end of the buffer.
        ///         Once the read completes, it will increase the <c>Count</c> with the number of received bytes.
        ///     </para>
        /// </remarks>
        public async Task<int> ReceiveAsync(IBufferSegment readBuffer)
        {
            if (_ioStream == null)
                throw new InvalidOperationException("Assign a socket first.");

            var usedCount = readBuffer.Offset - readBuffer.StartOffset;
            var read = await _ioStream.ReadAsync(readBuffer.Buffer, readBuffer.Offset, readBuffer.Capacity - usedCount);
            readBuffer.Count += read;
            return read;
        }

        //public async Task SendMoreAsync(byte[] buffer, int offset, int count, bool deliverIfChannelIsFree)
        //{
        //    _queuedOutboundPackets.Add(new SendPacketsElement(buffer, offset, count));
        //    if (_queuedOutboundPackets.Sum(x => x.Count) < MaxBytesPerWriteOperation || _socket.IsWritePending)
        //        return;

        //    await SendAllQueuedBuffers();
        //}


        private void Reset()
        {
            RemoteEndpoint = EmptyEndpoint.Instance;
            _state = ChannelState.Closed;
            if (_socket != null)
            {
                ChannelClosed?.Invoke(this, EventArgs.Empty);
                _socket.Dispose();
                _socket = null;
            }

            _ioStream.Close();
            _queuedOutboundPackets.Clear();
            foreach (var writeBuffer in _pooledWriteBuffers) writeBuffer.ReturnToPool();

            _pooledWriteBuffers.Clear();
        }

        private async Task SendAllQueuedBuffers()
        {
            foreach (var packet in _queuedOutboundPackets)
                await _ioStream.WriteAsync(packet.Buffer, packet.Offset, packet.Count);

            _queuedOutboundPackets.Clear();

            foreach (var writeBuffer in _pooledWriteBuffers) writeBuffer.ReturnToPool();
            _pooledWriteBuffers.Clear();
        }
    }
}