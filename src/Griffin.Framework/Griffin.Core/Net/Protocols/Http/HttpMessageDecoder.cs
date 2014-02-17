using System;
using System.IO;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    public class HttpMessageDecoder : IMessageDecoder
    {
        private int _frameContentBytesLeft = 0;
        private HeaderParser _headerParser;
        private bool _isHeaderParsed;
        private HttpMessage _message;
        private Action<object> _messageReceived;

        public HttpMessageDecoder()
        {
            _headerParser = new HeaderParser();
            _headerParser.HeaderParsed = OnHeader;
            _headerParser.RequestLineParsed = OnRequestLine;
            _headerParser.Completed = OnMessageParsed;
            _messageReceived = delegate { };
        }

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
                        MessageReceived(_message);
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
                    MessageReceived(_message);
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

        private void OnMessageParsed()
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

                _message = new BasicHttpResponse(code, part3, part1);
            }
            else
            {
                if (!part3.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException(
                        string.Format(
                            "Status line for requests should end with the HTTP version. Your line ended with '{0}'.",
                            part3));
                _message = new BasicHttpRequest(part1, part2, part3);
            }
        }
    }
}