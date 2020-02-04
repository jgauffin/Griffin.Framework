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
        private bool _isWebSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketDecoder"/> class.
        /// </summary>
        public WebSocketDecoder()
        {
            _httpMessageDecoder = new HttpMessageDecoder();
            _isWebSocket = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketDecoder"/> class.
        /// </summary>
        /// <param name="messageSerializer">Custom message serializer (typically inherits from <see cref="WebSocketDecoder"/>.)</param>
        public WebSocketDecoder(IMessageSerializer messageSerializer)
        {
            _httpMessageDecoder = new HttpMessageDecoder(messageSerializer);
            _isWebSocket = false;
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

            List<WebSocketFrame> frames = null;
            while (true)
            {
                if (buffer.BytesLeft() < 5)
                {
                    await buffer.ReceiveMore(channel);
                }

                var first = buffer.Buffer[buffer.Offset++];
                var second = buffer.Buffer[buffer.Offset++];

                var fin = (first & 0x80) == 0x80 ? WebSocketFin.Final : WebSocketFin.More;
                var rsv1 = (first & 0x40) == 0x40 ? WebSocketRsv.On : WebSocketRsv.Off;
                var rsv2 = (first & 0x20) == 0x20 ? WebSocketRsv.On : WebSocketRsv.Off;
                var rsv3 = (first & 0x10) == 0x10 ? WebSocketRsv.On : WebSocketRsv.Off;
                var opcode = (WebSocketOpcode) (first & 0x0f);
                var mask = (second & 0x80) == 0x80 ? WebSocketMask.Mask : WebSocketMask.Unmask;
                var payloadLen = (byte) (second & 0x7f);

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

                if (buffer.BytesLeft() < size)
                {
                    await buffer.ReceiveMore(channel);
                }

                var extPayloadLen = new byte[size];
                for (var i = 0; i < size; i++)
                {
                    extPayloadLen[i] = buffer.Buffer[buffer.Offset++];
                }

                var maskingKey = new byte[0];
                if (mask == WebSocketMask.Mask)
                {
                    maskingKey = new[]
                    {
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

                _frameContentBytesLeft = (int) len;
                if (buffer.BytesLeft() < _frameContentBytesLeft)
                {
                    await buffer.ReceiveMore(channel);
                }

                var frame = new WebSocketFrame(fin, rsv1, rsv2, rsv3, opcode, mask, maskingKey, payloadLen, extPayloadLen,
                    new MemoryStream(_frameContentBytesLeft));

                if (frame.Fin == WebSocketFin.More || frame.Opcode == WebSocketOpcode.Continuation)
                {
                    frames = new List<WebSocketFrame> {frame};
                }

                if (_frameContentBytesLeft > 0)
                {
                    var bytesToWrite = Math.Min(_frameContentBytesLeft, buffer.BytesLeft());
                    frame.Payload.Write(buffer.Buffer, buffer.Offset, bytesToWrite);
                    _frameContentBytesLeft -= bytesToWrite;
                    buffer.Offset += bytesToWrite;
                }

                if (_frameContentBytesLeft != 0) 
                    continue;

                frame.Payload.Position = 0;

                if (frame.Fin != WebSocketFin.Final) 
                    continue;

                if (frame.Opcode == WebSocketOpcode.Continuation)
                {
                    var mergedFrame = MergeMessages(frames);
                    return CreateMessage(mergedFrame);
                }

                return CreateMessage(frame);
            }
        }

        /// <summary>
        /// Reset decoder state so that we can decode a new message
        /// </summary>
        public void Clear()
        {
            _httpMessageDecoder.Clear();
            _handshake = null;
            _frameContentBytesLeft = 0;
            _isWebSocket = false;
        }

        private WebSocketFrame MergeMessages(IReadOnlyList<WebSocketFrame> frames)
        {
            if (frames.Count < 2)
                throw new InvalidOperationException("Merge two or more frames.");

            var first = frames[0];
            foreach (var frame in frames)
            {
                frame.Unmask();
                first.Payload.Position = first.Payload.Length;
                frame.Payload.CopyTo(first.Payload);
            }
            first.Payload.Position = 0;

            return first;
        }

        private WebSocketMessage CreateMessage(WebSocketFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            switch (_handshake)
            {
                // server mode
                case HttpRequest request:
                    return new WebSocketRequest(request, frame);

                // client mode
                case HttpResponse _:
                    return new WebSocketResponse(frame);

                default:
                    throw new InvalidOperationException("Unknown handshake.");
            }
        }


    }
}
