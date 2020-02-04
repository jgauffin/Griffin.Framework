using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Decodes websocket messages (once the HTTP handshake have been completed)
    /// </summary>
    public class WebSocketDecoder : IMessageDecoder
    {
        private readonly HttpMessageDecoder _httpMessageDecoder;
        private int _frameContentBytesLeft = 0;
        private HttpMessage _handshake;
        private WebSocketFrame _frame;
        private IList<WebSocketFrame> _frames;
        private Action<object> _messageReceived;
        private bool _isWebSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketDecoder"/> class.
        /// </summary>
        public WebSocketDecoder()
        {
            _httpMessageDecoder = new HttpMessageDecoder();
            _isWebSocket = false;
            _messageReceived = delegate { };
            _frames = new List<WebSocketFrame>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketDecoder"/> class.
        /// </summary>
        /// <param name="messageSerializer">Custom message serializer (typically inherits from <see cref="WebSocketDecoder"/>.)</param>
        public WebSocketDecoder(IMessageSerializer messageSerializer)
        {
            _httpMessageDecoder = new HttpMessageDecoder(messageSerializer);
            _isWebSocket = false;
            _messageReceived = delegate { };
            _frames = new List<WebSocketFrame>();
        }

        /// <summary>
        /// We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public async Task<object> DecodeAsync(IInboundBinaryChannel channel, IBufferSegment buffer)
        {
            if (!_isWebSocket)
            {
                // TODO: is there a better way to detect WebSocket upgrade?
                var msg = (HttpRequest)await _httpMessageDecoder.DecodeAsync(channel, buffer);
                if (!WebSocketUtils.IsWebSocketUpgrade(msg))
                    return msg;

                _handshake = msg;
                _isWebSocket = true;
                return msg;
            }

            while (true)
            {
                if (_frame == null)
                {
                    var first = buffer.Buffer[buffer.Offset++];
                    var second = buffer.Buffer[buffer.Offset++];

                    var fin = (first & 0x80) == 0x80 ? WebSocketFin.Final : WebSocketFin.More;
                    var rsv1 = (first & 0x40) == 0x40 ? WebSocketRsv.On : WebSocketRsv.Off;
                    var rsv2 = (first & 0x20) == 0x20 ? WebSocketRsv.On : WebSocketRsv.Off;
                    var rsv3 = (first & 0x10) == 0x10 ? WebSocketRsv.On : WebSocketRsv.Off;
                    var opcode = (WebSocketOpcode)(first & 0x0f);
                    var mask = (second & 0x80) == 0x80 ? WebSocketMask.Mask : WebSocketMask.Unmask;
                    var payloadLen = (byte)(second & 0x7f);

                    // TODO:
                    // check if valid headers
                    // control frame && payloadLen > 125
                    // control frame && more
                    // not data && compressed

                    var size = payloadLen < 126
                        ? 0
                        : payloadLen == 126
                            ? 2
                            : 8;

                    var extPayloadLen = new byte[size];
                    for (var i = 0; i < size; i++)
                    {
                        extPayloadLen[i] = buffer.Buffer[buffer.Offset++];
                    }

                    var maskingKey = new byte[0];
                    if (mask == WebSocketMask.Mask)
                    {
                        maskingKey = new[] {
                            buffer.Buffer[buffer.Offset++],
                            buffer.Buffer[buffer.Offset++],
                            buffer.Buffer[buffer.Offset++],
                            buffer.Buffer[buffer.Offset++],
                        };
                    }

                    ulong len = payloadLen < 126
                        ? payloadLen
                        : payloadLen == 126
                            ? WebSocketUtils.ToBigEndianUInt16(extPayloadLen)
                            : WebSocketUtils.ToBigEndianUInt64(extPayloadLen);

                    _frameContentBytesLeft = (int)len;

                    _frame = new WebSocketFrame(fin, rsv1, rsv2, rsv3, opcode, mask, maskingKey, payloadLen, extPayloadLen, new MemoryStream(_frameContentBytesLeft));

                    if (_frame.Fin == WebSocketFin.More || _frame.Opcode == WebSocketOpcode.Continuation)
                    {
                        _frames.Add(_frame);
                    }
                }

                if (_frameContentBytesLeft > 0)
                {
                    var bytesToWrite = Math.Min(_frameContentBytesLeft, buffer.BytesLeft());
                    _frame.Payload.Write(buffer.Buffer, buffer.Offset, bytesToWrite);
                    _frameContentBytesLeft -= bytesToWrite;
                    buffer.Offset += bytesToWrite;
                }

                if (_frameContentBytesLeft == 0)
                {
                    _frame.Payload.Position = 0;

                    if (_frame.Fin == WebSocketFin.Final)
                    {
                        if (_frame.Opcode == WebSocketOpcode.Continuation)
                        {
                            TriggerMessageReceived(_frames);
                            _frames = new List<WebSocketFrame>();
                        }
                        else
                        {
                            TriggerMessageReceived(new[] { _frame });
                        }
                    }
                    _frame = null;
                }

            }
        }

        /// <summary>
        /// Reset decoder state so that we can decode a new message
        /// </summary>
        public void Clear()
        {
            _httpMessageDecoder.Clear();
            _handshake = null;
            _frame = null;
            _frames = new List<WebSocketFrame>();
            _frameContentBytesLeft = 0;
            _isWebSocket = false;
        }

        private void TriggerMessageReceived(IEnumerable<WebSocketFrame> frames)
        {
            WebSocketFrame first = null;
            // combine payloads into the first frame
            foreach (WebSocketFrame frame in frames)
            {
                frame.Unmask();

                if (first == null)
                {
                    first = frame;
                    continue;
                }

                first.Payload.Position = first.Payload.Length;
                frame.Payload.CopyTo(first.Payload);
            }
            first.Payload.Position = 0;

            if (_handshake is HttpRequest) // server mode
            {
                _messageReceived(new WebSocketRequest((HttpRequest)_handshake, first));
            }
            else if (_handshake is HttpResponse) // client mode
            {
                _messageReceived(new WebSocketResponse(first));
            }
        }


    }
}
