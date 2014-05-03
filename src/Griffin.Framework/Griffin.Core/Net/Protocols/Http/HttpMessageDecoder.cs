using System;
using System.IO;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.BodyDecoders;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     Decodes HTTP messages
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Per default the body is not decoded. To change that behavior you should assign a decoder to the
    ///         <see cref="BodyDecoder" /> property.
    ///     </para>
    /// </remarks>
    public class HttpMessageDecoder : IMessageDecoder
    {
        private readonly HeaderParser _headerParser;
        private int _frameContentBytesLeft = 0;
        private bool _isHeaderParsed;
        private HttpMessage _message;
        private Action<object> _messageReceived;
        private HttpCookieParser _cookieParser = new HttpCookieParser();

        public HttpMessageDecoder()
        {
            _headerParser = new HeaderParser();
            _headerParser.HeaderParsed = OnHeader;
            _headerParser.RequestLineParsed = OnRequestLine;
            _headerParser.Completed = OnHeaderParsed;
            _messageReceived = delegate { };
        }

        public HttpMessageDecoder(IBodyDecoder decoder)
        {
            _headerParser = new HeaderParser();
            _headerParser.HeaderParsed = OnHeader;
            _headerParser.RequestLineParsed = OnRequestLine;
            _headerParser.Completed = OnHeaderParsed;
            BodyDecoder = decoder;
            _messageReceived = delegate { };
        }

        /// <summary>
        ///     Used to parse body
        /// </summary>
        public IBodyDecoder BodyDecoder { get; set; }

        /// <summary>
        ///     A message have been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    _messageReceived = m => { };
                else
                    _messageReceived = value;
            }
        }

        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            var receiveBufferOffset = buffer.Offset;
            var bytesLeftInReceiveBuffer = buffer.BytesTransferred;
            while (true)
            {
                if (bytesLeftInReceiveBuffer <= 0)
                    break;

                if (!_isHeaderParsed)
                {
                    var offsetBefore = receiveBufferOffset;
                    receiveBufferOffset = _headerParser.Parse(buffer, receiveBufferOffset);
                    if (!_isHeaderParsed)
                        return;

                    bytesLeftInReceiveBuffer -= receiveBufferOffset - offsetBefore;
                    _frameContentBytesLeft = _message.ContentLength;
                    if (_frameContentBytesLeft == 0)
                    {
                        TriggerMessageReceived(_message);
                        _message = null;
                        _isHeaderParsed = false;
                        continue;
                    }

                    _message.Body = new MemoryStream();
                }

                var bytesRead = BytesProcessed(buffer.Offset, receiveBufferOffset);
                var bytesToWrite = Math.Min(_frameContentBytesLeft, buffer.BytesTransferred - bytesRead);
                _message.Body.Write(buffer.Buffer, receiveBufferOffset, bytesToWrite);
                _frameContentBytesLeft -= bytesToWrite;
                receiveBufferOffset += bytesToWrite;
                bytesLeftInReceiveBuffer -= bytesToWrite;
                if (_frameContentBytesLeft == 0)
                {
                    _message.Body.Position = 0;
                    TriggerMessageReceived(_message);
                    Clear();
                }
            }
        }

        public void Clear()
        {
            _message = null;
            _isHeaderParsed = false;
            _frameContentBytesLeft = 0;
        }

        private int BytesProcessed(int startOffset, int currentOffset)
        {
            return currentOffset - startOffset;
        }

        private void OnHeader(string name, string value)
        {
            _message.AddHeader(name, value);
        }

        private void OnHeaderParsed()
        {
            _isHeaderParsed = true;
        }

        private void OnRequestLine(string part1, string part2, string part3)
        {
            if (part1.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
            {
                int code;
                if (!int.TryParse(part2, out code))
                    throw new BadRequestException(
                        string.Format("Second word in the status line should be a HTTP code, you specified '{0}'.",
                            part2));

                if (BodyDecoder != null)
                    _message = new HttpResponse(code, part3, part1);
                else
                    _message = new HttpResponseBase(code, part3, part1);
            }
            else
            {
                if (!part3.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException(
                        string.Format(
                            "Status line for requests should end with the HTTP version. Your line ended with '{0}'.",
                            part3));

                _message = BodyDecoder != null
                    ? new HttpRequest(part1, part2, part3)
                    : new HttpRequestBase(part1, part2, part3);
            }
        }

        private void TriggerMessageReceived(HttpMessage message)
        {
            var request = message as HttpRequest;
            if (BodyDecoder != null && request != null)
            {
                if (message.Body != null && message.Body.Length > 0)
                {
                    var result = BodyDecoder.Decode(request);
                    if (!result)
                        throw new BadRequestException("Unknown body content-type.");
                }
                var cookies = request.Headers["Cookie"];
                if (cookies != null)
                {
                    request.Cookies = _cookieParser.Parse(cookies);
                }
            }

            _messageReceived(message);
        }
    }
}