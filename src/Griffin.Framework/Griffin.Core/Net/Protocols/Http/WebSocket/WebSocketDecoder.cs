using System;
using System.Collections.Generic;
using System.IO;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketDecoder : IMessageDecoder
    {
        private readonly HttpMessageDecoder _httpMessageDecoder;
        private int _frameContentBytesLeft = 0;
        private IHttpMessage _handshake;
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
            _httpMessageDecoder.MessageReceived = OnHttpMessage;
            _isWebSocket = false;
            _messageReceived = delegate { };
            _frames = new List<WebSocketFrame>();
        }

        public WebSocketDecoder(IMessageSerializer messageSerializer)
        {
            _httpMessageDecoder = new HttpMessageDecoder(messageSerializer);
            _httpMessageDecoder.MessageReceived = OnHttpMessage;
            _isWebSocket = false;
            _messageReceived = delegate { };
            _frames = new List<WebSocketFrame>();
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
            set { _messageReceived = value ?? delegate { }; }
        }

        /// <summary>
        /// We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            if (!_isWebSocket)
            {
                _httpMessageDecoder.ProcessReadBytes(buffer);
            }
            else
            {
                var receiveBufferOffset = buffer.Offset;
                var bytesLeftInReceiveBuffer = buffer.BytesTransferred;
                while (true)
                {
                    if (bytesLeftInReceiveBuffer <= 0)
                        break;

                    if (_frame == null)
                    {
                        var first = buffer.Buffer[receiveBufferOffset + 0];
                        var second = buffer.Buffer[receiveBufferOffset + 1];

                        var fin = (first & 0x80) == 0x80 ? WebSocketFin.Final : WebSocketFin.More;
                        var rsv1 = (first & 0x40) == 0x40 ? WebSocketRsv.On : WebSocketRsv.Off;
                        var rsv2 = (first & 0x20) == 0x20 ? WebSocketRsv.On : WebSocketRsv.Off;
                        var rsv3 = (first & 0x10) == 0x10 ? WebSocketRsv.On : WebSocketRsv.Off;
                        var opcode = (WebSocketOpcode)(first & 0x0f);
                        var mask = (second & 0x80) == 0x80 ? WebSocketMask.Mask : WebSocketMask.Unmask;
                        var payloadLen = (byte)(second & 0x7f);

                        receiveBufferOffset += 2;
                        bytesLeftInReceiveBuffer -= 2;

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
                            extPayloadLen[i] = buffer.Buffer[receiveBufferOffset + i];
                        }
                        receiveBufferOffset += size;
                        bytesLeftInReceiveBuffer -= size;

                        var maskingKey = new byte[0];
                        if (mask == WebSocketMask.Mask)
                        {
                            maskingKey = new byte[] { 
                                buffer.Buffer[receiveBufferOffset + 0], 
                                buffer.Buffer[receiveBufferOffset + 1], 
                                buffer.Buffer[receiveBufferOffset + 2], 
                                buffer.Buffer[receiveBufferOffset + 3],
                            };
                            receiveBufferOffset += 4;
                            bytesLeftInReceiveBuffer -= 4;
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
                        var bytesRead = BytesProcessed(buffer.Offset, receiveBufferOffset);
                        var bytesToWrite = Math.Min(_frameContentBytesLeft, buffer.BytesTransferred - bytesRead);
                        _frame.Payload.Write(buffer.Buffer, receiveBufferOffset, bytesToWrite);
                        _frameContentBytesLeft -= bytesToWrite;
                        receiveBufferOffset += bytesToWrite;
                        bytesLeftInReceiveBuffer -= bytesToWrite;
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

        private int BytesProcessed(int startOffset, int currentOffset)
        {
            return currentOffset - startOffset;
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

            if (_handshake is IHttpRequest) // server mode
            {
                _messageReceived(new WebSocketRequest((IHttpRequest)_handshake, first));
            }
            else if (_handshake is IHttpResponse) // client mode
            {
                _messageReceived(new WebSocketResponse(first));
            }
        }

        /// <summary>
        /// Intercept http messages and look for websocket upgrade requests
        /// </summary>
        /// <param name="message">message from http decoder</param>
        private void OnHttpMessage(object message)
        {
            var httpMessage = message as IHttpMessage;
            // TODO: is there a better way to detect WebSocket upgrade?
            if (WebSocketUtils.IsWebSocketUpgrade(httpMessage))
            {
                _handshake = httpMessage;
                _isWebSocket = true;
            }

            _messageReceived(message);
        }

    }
}
