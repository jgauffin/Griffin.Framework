using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Protocols;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to secure the transport.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class SecureTcpChannel : ITcpChannel
    {
        private static readonly object CloseMessage = new object();
        private readonly SemaphoreSlim _closeEvent = new SemaphoreSlim(0, 1);
        private readonly IMessageDecoder _decoder;
        private readonly ISslStreamBuilder _sslStreamBuilder;
        private readonly IMessageEncoder _encoder;
        private readonly IMessageQueue _outboundMessages = new MessageQueue();
        private object _currentOutboundMessage;
        private DisconnectHandler _disconnectAction;
        private MessageHandler _messageReceived;
        private SocketBuffer _readBuffer;
        private MessageHandler _sendCompleteAction;
        private Socket _socket;
        private SslStream _stream;
        private SocketBuffer _writeBuffer;
        private BufferPreProcessorHandler _bufferPreProcessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <param name="encoder"></param>
        /// <param name="decoder"></param>
        /// <param name="sslStreamBuilder">Used to wrap the socket with a SSL stream</param>
        public SecureTcpChannel(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder, ISslStreamBuilder sslStreamBuilder)
        {
            if (readBuffer == null) throw new ArgumentNullException("readBuffer");
            if (encoder == null) throw new ArgumentNullException("encoder");
            if (decoder == null) throw new ArgumentNullException("decoder");
            if (sslStreamBuilder == null) throw new ArgumentNullException("sslStreamBuilder");

            _encoder = encoder;
            _decoder = decoder;
            _sslStreamBuilder = sslStreamBuilder;
            _decoder.MessageReceived = OnMessageReceived;

            _sendCompleteAction = (channel, message) => { };
            _disconnectAction = (channel, exception) => { };
            ChannelFailure = (channel, error) => HandleDisconnect(SocketError.ProtocolNotSupported);

            RemoteEndpoint = EmptyEndpoint.Instance;

            _readBuffer = new SocketBuffer(readBuffer);
            _writeBuffer = new SocketBuffer();
            ChannelId = GuidFactory.Create().ToString();
            Data = new ChannelData();
        }

        /// <summary>
        ///     Channel got disconnected
        /// </summary>
        public DisconnectHandler Disconnected
        {
            get { return _disconnectAction; }

            set
            {
                if (value == null)
                    _disconnectAction = (x, e) => { };
                else
                    _disconnectAction = value;
            }
        }

        /// <summary>
        ///     Channel received a new message
        /// </summary>
        public MessageHandler MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    throw new ArgumentException("You must have a MessageReceived delegate");


                _messageReceived = value;
            }
        }

        /// <summary>
        ///     Channel have sent a message
        /// </summary>
        public MessageHandler MessageSent
        {
            get { return _sendCompleteAction; }

            set
            {
                if (value == null)
                {
                    _sendCompleteAction = (x, y) => { };
                    return;
                }

                _sendCompleteAction = value;
            }
        }

        /// <summary>
        /// Invoked if the decoder failes to handle an incoming message
        /// </summary>
        /// <remarks>
        /// <para>
        /// The handler MUST close the connection once a reply have been sent.
        /// </para>
        /// </remarks>
        public ChannelFailureHandler ChannelFailure { get; set; }

        /// <summary>
        /// Checks if the channel is connected.
        /// </summary>
        public bool IsConnected { get { return _socket.Connected; }}

        /// <summary>
        ///     Gets address of the connected end point.
        /// </summary>
        public EndPoint RemoteEndpoint { get; private set; }

        /// <summary>
        /// Identity of this channel
        /// </summary>
        /// <remarks>
        /// Must be unique within a server.
        /// </remarks>
        public string ChannelId { get; private set; }

        /// <summary>
        ///     Assign a socket to this channel
        /// </summary>
        /// <param name="socket">Connected socket</param>
        /// <remarks>
        ///     the channel will start receive new messages as soon as you've called assign.
        /// </remarks>
        public void Assign(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            if (MessageReceived == null)
                throw new InvalidOperationException("Must handle the MessageReceived callback before invoking this method.");

            _socket = socket;
            _stream = _sslStreamBuilder.Build(this, socket);
            _stream.BeginRead(_readBuffer.Buffer, _readBuffer.Offset, _readBuffer.Capacity, OnReadCompleted, null);
        }

        /// <summary>
        ///     Send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     <para>
        ///         Outbound messages are enqueued and sent in order.
        ///     </para>
        ///     <para>
        ///         You may enqueue <c>byte[]</c> arrays or <see cref="Stream" />  objects. They will not be serialized but
        ///         MicroMessage framed directly.
        ///     </para>
        /// </remarks>
        public void Send(object message)
        {
            lock (_outboundMessages)
            {
                if (_currentOutboundMessage != null)
                {
                    _outboundMessages.Enqueue(message);
                    return;
                }

                _currentOutboundMessage = message;
            }

            SendCurrent();
        }

        /// <summary>
        /// Can be used to store information in the channel so that you can access it at later requests.
        /// </summary>
        /// <remarks>
        /// <para>All data is lost when the channel is closed.</para>
        /// </remarks>
        public IChannelData Data { get; private set; }


        /// <summary>
        /// Pre processes incoming bytes before they are passed to the message builder.
        /// </summary>
        /// <remarks>
        /// Can be used if you for instance uses a custom authentication mechanism which requires to process incoming
        /// bytes.
        /// </remarks>
        public BufferPreProcessorHandler BufferPreProcessor
        {
            get { return _bufferPreProcessor; }
            set { _bufferPreProcessor = value; }
        }

        /// <summary>
        /// Queue used to store messages that are going to be sent to the remote end point.
        /// </summary>
        /// <remarks>
        /// <para>We use a queue so that the caller can fire and forget. We send all messages in order as soon as possible.</para>
        /// </remarks>
        public IMessageQueue OutboundMessageQueue { get; set; }

        /// <summary>
        /// Close channel, wait for all messages to be sent.
        /// </summary>
        public void Close()
        {
            Send(CloseMessage);
            _closeEvent.Wait(5000);
        }

        /// <summary>
        ///     Signal channel to close.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will wait for all data to be sent before closing.
        ///     </para>
        /// </remarks>
        public Task CloseAsync()
        {
            Send(CloseMessage);
            var t = _closeEvent.WaitAsync(5000);

            // release again so that we can take reuse it internally
            t.ContinueWith(x => _closeEvent.Release());

            return t;
        }

        /// <summary>
        ///     Cleanup everything so that the channel can be resused.
        /// </summary>
        public void Cleanup()
        {
            _encoder.Clear();
            _decoder.Clear();
            _currentOutboundMessage = null;
            _socket = null;

            if (Data != null)
                Data.Clear();

            RemoteEndpoint = EmptyEndpoint.Instance;

            object msg;
            while (_outboundMessages.TryDequeue(out msg))
            {
            }
        }


        private void HandleDisconnect(SocketError socketError)
        {
            _disconnectAction(this, new SocketException((int) socketError));
            Cleanup();
        }

        private void HandleDisconnect(Exception exception)
        {
            _disconnectAction(this, exception);
            Cleanup();
        }


        private void OnMessageReceived(object obj)
        {
            MessageReceived(this, obj);
        }

        private void OnReadCompleted(IAsyncResult ar)
        {
            var readBytes = 0;
            try
            {
                readBytes = _stream.EndRead(ar);
            }
            catch (Exception ex)
            {
                HandleDisconnect(ex);
            }

            if (readBytes == 0)
            {
                HandleDisconnect(SocketError.ConnectionReset);
                return;
            }

            _readBuffer.BytesTransferred = readBytes;
            _readBuffer.Offset = _readBuffer.BaseOffset;
            _readBuffer.Count = readBytes;
            if (_bufferPreProcessor != null)
            {
                var read = _bufferPreProcessor(this, _readBuffer);
                _readBuffer.Count -= read;
                _readBuffer.Offset += read;
            }

            try
            {
                if (_readBuffer.Count > 0)
                    _decoder.ProcessReadBytes(_readBuffer);
            }
            catch (Exception exception)
            {
                ChannelFailure(this, exception);
                return;
            }

            ReadAsync();
        }

        private void OnSendCompleted(IAsyncResult ar)
        {
            try
            {
                _stream.EndWrite(ar);
            }
            catch (Exception)
            {
                // ignore errors, let the receiving end take care of that.
                return;
            }

            var isComplete = _encoder.OnSendCompleted(_writeBuffer.Count);
            if (!isComplete)
            {
                _encoder.Send(_writeBuffer);
                _stream.BeginWrite(_writeBuffer.Buffer, _writeBuffer.Offset, _writeBuffer.Count, OnSendCompleted, null);
                return;
            }

            var msg = _currentOutboundMessage;

            lock (_outboundMessages)
            {
                if (!_outboundMessages.TryDequeue(out _currentOutboundMessage))
                {
                    _currentOutboundMessage = null;
                    _sendCompleteAction(this, msg);
                    return;
                }
            }

            _sendCompleteAction(this, msg);
            SendCurrent();
        }

        private void ReadAsync()
        {
            _stream.BeginRead(_readBuffer.Buffer, _readBuffer.Offset, _readBuffer.Capacity, OnReadCompleted, null);
        }

        private void SendCurrent()
        {
            // Allows us to send everything before closing the connection.
            if (_currentOutboundMessage == CloseMessage)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _currentOutboundMessage = null;
                _closeEvent.Release();
                return;
            }


            _encoder.Prepare(_currentOutboundMessage);
            _encoder.Send(_writeBuffer);
            _stream.BeginWrite(_writeBuffer.Buffer, _writeBuffer.Offset, _writeBuffer.Count, OnSendCompleted, null);
        }
    }
}