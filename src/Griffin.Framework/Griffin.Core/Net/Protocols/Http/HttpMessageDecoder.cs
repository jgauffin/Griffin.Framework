using System;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.Messages;
using Griffin.Net.Protocols.Http.Serializers;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     Decodes HTTP messages
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Per default the body is not decoded. To change that behavior you should use the constructor that takes
    ///         a message serializer.
    ///     </para>
    /// </remarks>
    public class HttpMessageDecoder : IMessageDecoder
    {
        private readonly HttpCookieParser _cookieParser = new HttpCookieParser();
        private readonly HeaderParser _headerParser;
        private readonly IMessageSerializer _messageSerializer;
        private HttpMessage _message;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpMessageDecoder" /> class.
        /// </summary>
        public HttpMessageDecoder()
        {
            _headerParser = new HeaderParser
            {
                HeaderParsed = OnHeader,
                RequestLineParsed = OnRequestLine
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpMessageDecoder" /> class.
        /// </summary>
        /// <param name="messageSerializer">The message serializer.</param>
        /// <exception cref="System.ArgumentNullException">messageSerializer</exception>
        public HttpMessageDecoder(IMessageSerializer messageSerializer)
        : this()
        {
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        }

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment buffer)
        {
            if (buffer.BytesLeft() <= 0)
                await channel.ReceiveAsync(buffer);

            await _headerParser.Parse(buffer, channel);
            if (_message.ContentLength == 0)
            {
                PrepareMessageForDelivery(_message);
                var msg2 = _message;
                Clear();
                return msg2;
            }

            _message.Body = new MemoryStream();

            var contentBytesLeft = _message.ContentLength;
            while (true)
            {
                var bytesToWrite = Math.Min(contentBytesLeft, buffer.BytesLeft());
                _message.Body.Write(buffer.Buffer, buffer.Offset, bytesToWrite);
                buffer.Offset += bytesToWrite;
                contentBytesLeft -= bytesToWrite;

                if (contentBytesLeft == 0)
                    break;

                buffer.Offset = buffer.StartOffset;
                buffer.Count = 0;
                await channel.ReceiveAsync(buffer);
                if (buffer.Count == 0)
                    throw new ParseException("Channel got disconnected.");
            }

            _message.Body.Position = 0;
            PrepareMessageForDelivery(_message);
            var msg = _message;
            Clear();
            return msg;
        }

        /// <summary>
        ///     Reset decoder state so that we can decode a new message
        /// </summary>
        public void Clear()
        {
            _message = null;
            _headerParser.Reset();
        }

        private void OnHeader(string name, string value)
        {
            _message.AddHeader(name, value);
        }

        private void OnRequestLine(string part1, string part2, string part3)
        {
            if (part1.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(part2, out var code))
                    throw new BadRequestException(
                        $"Second word in the status line should be a HTTP code, you specified '{part2}'.");

                _message = new HttpResponse(code, part3, part1);
            }
            else
            {
                if (!part3.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException(
                        $"Status line for requests should end with the HTTP version. Your line ended with '{part3}'.");

                _message = new HttpRequest(part1, part2, part3);
            }
        }

        private void PrepareMessageForDelivery(HttpMessage message)
        {
            if (_messageSerializer == null || !(message is HttpRequest request) || _messageSerializer.SupportedContentTypes.Length == 0)
                return;

            if (message.Body == null || message.Body.Length <= 0)
            {
                return;
            }

            var result = _messageSerializer.Deserialize(message.Headers["Content-Type"], message.Body) as HttpContent;
            if (result == null)
            {
                //it's a so simple protocol, we can expect that the client can handle it.
                if (!"text/plain".Equals(message.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Unsupported content-type: " + message.ContentType);
            }
            else
            {
                message.Content = result;
            }

            var cookies = request.Headers["Cookie"];
            if (cookies != null)
            {
                request.Cookies = _cookieParser.Parse(cookies);
            }
        }
    }
}