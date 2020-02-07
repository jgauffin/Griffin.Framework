using Griffin.Net.Channels;
using System;
using System.IO;
using System.Threading.Tasks;
using Griffin.Net.Buffers;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Encodes web socket messages over  HTTP
    /// </summary>
    public class WebSocketEncoder : IMessageEncoder
    {
        private readonly HttpMessageEncoder _httpMessageEncoder;
        private HttpMessage _handshake;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketEncoder"/> class.
        /// </summary>
        public WebSocketEncoder()
        {
            _httpMessageEncoder = new HttpMessageEncoder();
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        public async Task SendAsync(WebSocketMessage message, IBinaryChannel channel)
        {
            var offset = (int)message.Payload.Position;
            var length = (int)message.Payload.Length;
            var frameLength = length - offset;

            var fin = WebSocketFin.Final;
            if (frameLength > WebSocketFrame.FragmentLength)
            {
                frameLength = WebSocketFrame.FragmentLength;
                fin = WebSocketFin.More;
            }
            var opcode = WebSocketOpcode.Continuation;
            if (offset == 0) // first frame
            {
                opcode = message.OpCode;
            }

            var buff = new byte[frameLength];
            message.Payload.Read(buff, 0, buff.Length);
            var payload = new MemoryStream(buff, true);

            var frame = new WebSocketFrame(fin, opcode, (_handshake is HttpRequest) ? WebSocketMask.Mask : WebSocketMask.Unmask, payload);
            using (var stream = new MemoryStream())
            {
                var header = (int)frame.Fin;
                header = (header << 1) + (int)frame.Rsv1;
                header = (header << 1) + (int)frame.Rsv2;
                header = (header << 1) + (int)frame.Rsv3;
                header = (header << 4) + (int)frame.Opcode;
                header = (header << 1) + (int)frame.Mask;
                header = (header << 7) + (int)frame.PayloadLength;

                stream.Write(WebSocketUtils.GetBigEndianBytes((ushort)header), 0, 2);

                if (frame.PayloadLength > 125)
                {
                    stream.Write(frame.ExtPayloadLength, 0, frame.ExtPayloadLength.Length);
                }

                if (frame.Mask == WebSocketMask.Mask)
                {
                    stream.Write(frame.MaskingKey, 0, frame.MaskingKey.Length);
                    frame.Unmask();
                }

                if (frame.PayloadLength > 0)
                {
                    frame.Payload.CopyTo(stream);
                }

                await channel.SendAsync(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public async Task EncodeAsync(object message, IBinaryChannel channel)
        {
            try
            {
                if (message is HttpMessage httpMessage)
                {
                    if (WebSocketUtils.IsWebSocketUpgrade(httpMessage))
                    {
                        _handshake = httpMessage;
                    }

                    await _httpMessageEncoder.EncodeAsync(httpMessage, channel);
                    return;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("This encoder only supports messages deriving from 'HttpMessage' or 'WebSocketMessage'", e);
            }

            await SendAsync((WebSocketMessage)message, channel);
        }

        /// <summary>
        /// Reset encoder state for a new HTTP request.
        /// </summary>
        public void Clear()
        {
            _httpMessageEncoder.Clear();
        }
    }
}
