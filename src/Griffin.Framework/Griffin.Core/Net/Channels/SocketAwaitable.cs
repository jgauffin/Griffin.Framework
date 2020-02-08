using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin.Net.Channels
{
    public sealed class SocketAwaitable : INotifyCompletion
    {
        private static readonly Action SENTINEL = () => { };
        private readonly SocketAsyncEventArgs _eventArgs;
        private Action _continuation;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            _eventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
            eventArgs.Completed += delegate
            {
                var prev = _continuation ?? Interlocked.CompareExchange(
                               ref _continuation, SENTINEL, null);
                prev?.Invoke();
            };
        }

        public bool IsCompleted { get; private set; }

        public bool Pending { get; private set; }

        public void OnCompleted(Action continuation)
        {
            if (_continuation == SENTINEL ||
                Interlocked.CompareExchange(
                    ref _continuation, continuation, null) == SENTINEL)
            {
                Pending = false;
                Task.Run(continuation);
            }
        }

        public SocketAwaitable ConnectAsync(Socket socket, EndPoint remoteEndPoint)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (remoteEndPoint == null) throw new ArgumentNullException(nameof(remoteEndPoint));

            Pending = true;
            Reset();
            _eventArgs.RemoteEndPoint = remoteEndPoint;
            Pending = socket.ConnectAsync(_eventArgs);
            if (!Pending) IsCompleted = true;

            return this;
        }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            if (_eventArgs.SocketError != SocketError.Success)
                throw new SocketException((int) _eventArgs.SocketError);
        }

        public SocketAwaitable ReceiveAsync(Socket socket)
        {
            Reset();
            Pending = socket.ReceiveAsync(_eventArgs);
            if (!Pending) IsCompleted = true;

            return this;
        }

        public void Reset()
        {
            // set when connecting
            _eventArgs.RemoteEndPoint = null;

            Pending = false;
            IsCompleted = false;
            _continuation = null;
        }

        public SocketAwaitable SendAsync(Socket socket)
        {
            Reset();
            Pending = socket.SendAsync(_eventArgs);
            if (!Pending) IsCompleted = true;

            return this;
        }
    }
}