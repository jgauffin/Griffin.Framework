using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    internal class FakeChannel : IBinaryChannel
    {
        private readonly MemoryStream _stream = new MemoryStream();

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            await _stream.WriteAsync(buffer, offset, count);
        }

        public async Task SendAsync(IEnumerable<IBufferSegment> buffers)
        {
            foreach (var buffer in buffers) await _stream.WriteAsync(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public async Task SendAsync(IBufferSegment buffer)
        {
            await _stream.WriteAsync(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public async Task SendMoreAsync(byte[] buffer, int offset, int count)
        {
            await _stream.WriteAsync(buffer, offset, count);
        }

        public async Task SendMoreAsync(IBufferSegment buffer)
        {
            await _stream.WriteAsync(buffer.Buffer, buffer.Offset, buffer.Count);
        }

        public async Task<int> ReceiveAsync(IBufferSegment readBuffer)
        {
            var count= await _stream.ReadAsync(readBuffer.Buffer, readBuffer.Offset,
                readBuffer.Capacity - readBuffer.Count);
            readBuffer.Count += count;
            return count;
        }

        public IChannelData ChannelData { get; } = new ChannelData();
        public bool IsConnected { get; }
        public bool IsOpen { get; }
        public int MaxBytesPerWriteOperation { get; set; }
        public EndPoint RemoteEndpoint { get; }
        public object UserToken { get; set; }
        public event EventHandler ChannelClosed;

        public Task CloseAsync()
        {
            _stream.SetLength(0);
            return Task.CompletedTask;
        }

        public void Assign(Socket socket)
        {
        }

        public Task OpenAsync()
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(EndPoint endpoint)
        {
            return Task.CompletedTask;
        }

        public void Reset()
        {
            _stream.SetLength(0);
        }

        public void ResetPosition()
        {
            _stream.Flush();
            _stream.Position = 0;
        }
    }
}