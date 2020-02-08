using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net
{
    public class AsyncSocket : IDisposable, IInboundBinaryChannel
    {
        private readonly SocketAsyncEventArgs _readArgs;
        private readonly SocketAwaitable _readAwaitable;
        private readonly SocketAsyncEventArgs _writeArgs;
        private readonly SocketAwaitable _writeAwaitable;
        private long _receiveIsPending;
        private Socket _socket;

        public AsyncSocket()
        {
            _readArgs = new SocketAsyncEventArgs();
            _readAwaitable = new SocketAwaitable(_readArgs);

            _writeArgs = new SocketAsyncEventArgs();
            _writeAwaitable = new SocketAwaitable(_writeArgs);
        }

        public bool IsWritePending => _writeAwaitable.Pending;
        public bool IsConnected => _socket?.Connected == true;

        public void Dispose()
        {
            _readArgs?.Dispose();
            _writeArgs?.Dispose();
            _socket?.Dispose();
            _socket = null;
        }

        public void Assign(Socket socket)
        {
            if (!socket.Connected)
                throw new InvalidOperationException("Expected socket to be connected.");
            _socket = socket;
        }

        public void Close()
        {
            _socket?.Close();
            _socket = null;
            _receiveIsPending = 0;
        }

        public async Task CloseAsync()
        {
            _socket.Shutdown(SocketShutdown.Both);
            if (Interlocked.Read(ref _receiveIsPending) == 1) await _readAwaitable;

            _socket.Close();
        }

        public async Task ConnectAsync(EndPoint remoteEndPoint)
        {
            if (_socket != null)
            {
                throw new InvalidOperationException(
                    "Expected socket to be NULL (i.e. this AsyncSocket have been reset).");
            }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _writeAwaitable.ConnectAsync(_socket, remoteEndPoint);
        }

        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            Interlocked.Exchange(ref _receiveIsPending, 1);
            _readArgs.SetBuffer(buffer, offset, count);
            await _readAwaitable.ReceiveAsync(_socket);
            return _readArgs.BytesTransferred;
        }

        public async Task<int> ReceiveAsync(IBufferSegment buffer)
        {
            Interlocked.Exchange(ref _receiveIsPending, 1);
            var len = buffer.Capacity - buffer.Count;
            _readArgs.SetBuffer(buffer.Buffer, buffer.Offset, len);
            await _readAwaitable.ReceiveAsync(_socket);

            buffer.Count += _readArgs.BytesTransferred;

            return _readArgs.BytesTransferred;
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            _writeArgs.SetBuffer(buffer, offset, count);
            await _writeAwaitable.SendAsync(_socket);

            while (_writeArgs.BytesTransferred < count)
            {
                count -= _writeArgs.BytesTransferred;
                offset += _writeArgs.BytesTransferred;

                _writeArgs.SetBuffer(buffer, offset, count);
                await _writeAwaitable.SendAsync(_socket);
            }
        }

        public async Task SendAsync(SendPacketsElement[] packets)
        {
            _writeArgs.SendPacketsElements = packets;
            await _writeAwaitable.SendAsync(_socket);
        }

        /// <summary>
        ///     Shutdown receive operations.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Once done, wait for your <see cref="ReceiveAsync" /> to be aborted and then close the socket.
        ///     </para>
        /// </remarks>
        public void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Receive);
        }
    }
}