using System;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     Used to encode request/response into a byte stream.
    /// </summary>
    public class HttpMessageEncoder : IMessageEncoder
    {
        private readonly IBufferSegment _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageEncoder"/> class.
        /// </summary>
        public HttpMessageEncoder()
        {
            _buffer = new StandAloneBuffer(65535);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageEncoder"/> class.
        /// </summary>
        public HttpMessageEncoder(IBufferSegment buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        public async Task EncodeAsync(object message, IBinaryChannel channel)
        {
            if (!(message is HttpMessage httpMessage))
                throw new InvalidOperationException("This encoder only supports messages deriving from 'HttpMessage'");

            httpMessage = (HttpMessage)message;
            if (httpMessage.Body == null || httpMessage.Body.Length == 0)
                httpMessage.Headers["Content-Length"] = "0";
            else if (httpMessage.ContentLength == 0)
            {
                httpMessage.ContentLength = (int)httpMessage.Body.Length;
                if (httpMessage.Body.Position == httpMessage.Body.Length)
                    httpMessage.Body.Position = 0;
            }

            _buffer.WriteString(httpMessage.StatusLine);
            _buffer.WriteString("\r\n");
            foreach (var header in httpMessage.Headers)
            {
                _buffer.WriteString($"{header.Key}: {header.Value}\r\n");
            }
            _buffer.WriteString("\r\n");

            if (httpMessage.Body == null || httpMessage.ContentLength == 0)
            {
                _buffer.Count = _buffer.Offset;
                _buffer.Offset = _buffer.StartOffset;
                await channel.SendAsync(_buffer);
                return;
            }

            var bytesLeft = httpMessage.ContentLength;

            // need to specify count now since everything builds on appending to offset.
            _buffer.Count = _buffer.Offset - _buffer.StartOffset;

            while (true)
            {
                var toSend = Math.Min(_buffer.UnallocatedBytes(), bytesLeft);
                var read = await httpMessage.Body.ReadAsync(_buffer.Buffer, _buffer.Offset, toSend);
                bytesLeft -= read;

                _buffer.Count += read;
                _buffer.Offset = _buffer.StartOffset;
                var http = Encoding.UTF8.GetString(_buffer.Buffer, _buffer.StartOffset, _buffer.Count);
                await channel.SendAsync(_buffer);

                if (bytesLeft == 0)
                    break;

                _buffer.Offset = _buffer.StartOffset;
                _buffer.Count = 0;
            }
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            _buffer.Offset = _buffer.StartOffset;
            _buffer.Count = 0;
        }
    }
}