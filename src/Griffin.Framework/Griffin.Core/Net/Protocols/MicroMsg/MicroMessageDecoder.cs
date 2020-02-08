using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    ///     Decode messages encoded with <see cref="MicroMessageEncoder" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         As <see cref="MicroMessageEncoder" /> can out-of-the-box send <c>Stream</c>-derived classes and <c>byte[]</c>
    ///         arrays this class
    ///         has to handle that too.
    ///     </para>
    ///     <para>
    ///         Streams will always be either <c>MemoryStream</c> or <c>FileStream</c> depending of the content-length. Same
    ///         things goes for messages
    ///         which are sent as <c>byte[]</c> arrays. They will also be received as streams.
    ///     </para>
    /// </remarks>
    public class MicroMessageDecoder : IMessageDecoder
    {
        /// <summary>
        ///     Protocol version
        /// </summary>
        public const byte Version = 1;

        /// <summary>
        ///     Size of the fixed header: version (1), content length (4), type name length (1) = 6
        /// </summary>
        /// <remarks>
        ///     The header size field is not included in the actual header count as it always have to be read to
        ///     get the actual header size.
        /// </remarks>
        public const int FixedHeaderLength = sizeof(byte) + sizeof(int) + sizeof(byte);

        private readonly Stream _contentStream = new MemoryStream();

        private readonly IMessageSerializer _serializer;
        private bool _completed;
        private int _contentLength;
        private string _contentName;
        private short _headerSize;
        private object _message;
        private byte _protocolVersion;
        private Func<IInboundBinaryChannel, IBufferSegment, Task> _stateMethod;
        private int _typeLength;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageDecoder" /> class.
        /// </summary>
        /// <param name="serializer">The serializer used to decode the message that is being transported with MicroMsg.</param>
        /// <exception cref="System.ArgumentNullException">serializer</exception>
        public MicroMessageDecoder(IMessageSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _stateMethod = ProcessFixedHeader;
        }

        /// <summary>
        ///     Process bytes that we've received on the socket.
        /// </summary>
        /// <param name="channel">Channel used to read data.</param>
        /// <param name="receiveBuffer">Contains data to decode</param>
        /// <remarks>
        ///     <para>
        ///         The decoder will read more from the socket when required.
        ///     </para>
        /// </remarks>
        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment receiveBuffer)
        {
            _completed = false;
            while (!_completed) await _stateMethod(channel, receiveBuffer);

            return _message;
        }

        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment receiveBuffer, CancellationToken token)
        {
            _completed = false;
            while (!_completed)
            {
                token.ThrowIfCancellationRequested();

                await _stateMethod(channel, receiveBuffer);
            }

            return _message;
        }


        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment receiveBuffer, TimeSpan timeout)
        {
            _completed = false;
            var sw = Stopwatch.StartNew();
            while (!_completed && sw.Elapsed < timeout)
                await _stateMethod(channel, receiveBuffer);

            if (sw.Elapsed > timeout)
                return null;

            return _message;
        }

        /// <summary>
        ///     Reset the decoder so that we can parse a new message
        /// </summary>
        public void Clear()
        {
            _contentLength = 0;
            _contentName = "";
            _contentStream.SetLength(0);
            _stateMethod = ProcessFixedHeader;
        }

        private void CopyBytes(IBufferSegment e)
        {
            var bytesToCopy = e.BytesLeft();
            Buffer.BlockCopy(e.Buffer, e.Offset, e.Buffer, e.StartOffset, bytesToCopy);
            e.Offset = e.StartOffset;
            e.Count = bytesToCopy;
        }

        private async Task EnsureBytes(IInboundBinaryChannel channel, IBufferSegment buffer, int byteCount)
        {
            if (buffer.BytesLeft() >= byteCount)
                return;

            if (buffer.UnallocatedBytes() < byteCount)
                CopyBytes(buffer);

            var safeguard = Math.Min(30, byteCount);
            var before = buffer.Count;
            while (buffer.BytesLeft() < byteCount && --safeguard > 0)
            {
                await channel.ReceiveAsync(buffer);
            }

            if (safeguard == 0 && before == buffer.Count)
                throw new DecoderFailureException("Failed to receive required bytes (did several attempts).");
        }

        private async Task ProcessContent(IInboundBinaryChannel channel, IBufferSegment buffer)
        {
            var bytesLeft = _contentLength;
            while (bytesLeft > 0)
            {
                await EnsureBytes(channel, buffer, bytesLeft);

                var bytesToWrite = Math.Min(bytesLeft, buffer.BytesLeft());
                _contentStream.Write(buffer.Buffer, buffer.Offset, bytesToWrite);
                buffer.Offset += bytesToWrite;
                bytesLeft -= bytesToWrite;
            }

            _stateMethod = ProcessFixedHeader;
            _contentStream.Position = 0;

            _completed = true;
            if (_contentName == "stream")
                _message = _contentStream;
            else if (_contentName == "byte[]")
                _message = _contentStream;
            else
                _message = _serializer.Deserialize(_contentName, _contentStream);
        }

        private async Task ProcessFixedHeader(IInboundBinaryChannel channel, IBufferSegment buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            await EnsureBytes(channel, buffer, FixedHeaderLength + 2);

            _headerSize = BitConverter.ToInt16(buffer.Buffer, buffer.Offset);
            _protocolVersion = buffer.Buffer[buffer.Offset + 2];
            _contentLength = BitConverter.ToInt32(buffer.Buffer, buffer.Offset + 3);
            _typeLength = buffer.Buffer[buffer.Offset + 7];
            buffer.Offset += 8;

            await EnsureBytes(channel, buffer, 4);
            _contentName = Encoding.ASCII.GetString(buffer.Buffer, buffer.Offset, _typeLength);
            buffer.Offset += _typeLength;

            _stateMethod = ProcessContent;
            _contentStream.Position = 0;
            _contentStream.SetLength(0);
        }
    }
}