using Griffin.Net.Channels;
using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// Encodes web socket messages over  HTTP
    /// </summary>
    public class WebSocketEncoder : IMessageEncoder
    {
        private readonly HttpMessageEncoder _httpMessageEncoder;
        private byte[] _buffer;
        private int _bytesToSend;
        private IWebSocketMessage _message;
        private int _offset;
        private int _totalAmountToSend;
        private IHttpMessage _handshake;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketEncoder"/> class.
        /// </summary>
        public WebSocketEncoder()
        {
            _httpMessageEncoder = new HttpMessageEncoder();
        }

        /// <summary>
        ///     Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message)
        {
            if (message is IWebSocketMessage)
            {
                _message = (IWebSocketMessage)message;
                _message.Payload.Position = 0;
                _totalAmountToSend = (int)_message.Payload.Length;
            }
            else
            {
                try
                {
                    _httpMessageEncoder.Prepare(message);

                    var httpMessage = message as IHttpMessage;
                    if (WebSocketUtils.IsWebSocketUpgrade(httpMessage))
                    {
                        _handshake = httpMessage;
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("This encoder only supports messages deriving from 'HttpMessage' or 'WebSocketMessage'", e);
                }
            }
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        public void Send(ISocketBuffer buffer)
        {
            if (_message == null)
            {
                _httpMessageEncoder.Send(buffer);
            }
            else
            {
                // last send operation did not send all bytes enqueued in the buffer
                // so let's just continue until doing next message
                if (_bytesToSend > 0)
                {
                    buffer.SetBuffer(_buffer, _offset, _bytesToSend);
                    return;
                }

                var offset = (int)_message.Payload.Position;
                var length = (int)_message.Payload.Length;
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
                    opcode = _message.Opcode;
                }

                var buff = new byte[frameLength];
                _message.Payload.Read(buff, 0, buff.Length);
                var payload = new MemoryStream(buff);

                WebSocketFrame frame = new WebSocketFrame(fin, opcode, (_handshake is IHttpRequest) ? WebSocketMask.Mask : WebSocketMask.Unmask, payload);

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

                    _totalAmountToSend += (int)stream.Length;

                    if (frame.PayloadLength > 0)
                    {
                        frame.Payload.CopyTo(stream);
                    }

                    buffer.UserToken = _message;

                    _buffer = stream.ToArray();
                    _bytesToSend = _buffer.Length;

                    buffer.SetBuffer(_buffer, 0, _bytesToSend);
                }
            }
        }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred)
        {
            if (_message == null)
            {
                return _httpMessageEncoder.OnSendCompleted(bytesTransferred);
            }

            _totalAmountToSend -= bytesTransferred;
            _bytesToSend -= bytesTransferred;
            _offset += bytesTransferred;

            if (_bytesToSend <= 0)
                _offset = 0;

            if (_totalAmountToSend == 0)
            {
                Clear();
            }

            return _totalAmountToSend <= 0;
        }

        /// <summary>
        /// Reset encoder state for a new HTTP request.
        /// </summary>
        public void Clear()
        {
            _httpMessageEncoder.Clear();

            if (_message != null && _message.Payload != null)
                _message.Payload.Dispose();

            _bytesToSend = 0;
            _message = null;
        }
    }
}
