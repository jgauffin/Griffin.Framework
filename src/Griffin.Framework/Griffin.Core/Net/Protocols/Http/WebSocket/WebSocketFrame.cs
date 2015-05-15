using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    /// <summary>
    /// WebSocket frame
    /// </summary>
    /// <remarks>
    /// <para>
    /// The specification for this frame can be found at http://tools.ietf.org/html/rfc6455#section-5.2.
    /// </para>
    /// </remarks>
    internal class WebSocketFrame
    {
        /// <summary>
        /// Maximum fragment length. (<value>65535</value>)
        /// Can be up to UInt64.MaxValue
        /// </summary>
        public const int FragmentLength = 65535; // 1016;

        private WebSocketFin _fin;
        private WebSocketRsv _rsv1;
        private WebSocketRsv _rsv2;
        private WebSocketRsv _rsv3;
        private WebSocketOpcode _opcode;
        private WebSocketMask _mask;
        private byte _payloadLength;
        private byte[] _extPayloadLength;
        private byte[] _maskingKey;
        private Stream _payload;

        public WebSocketFrame(
            WebSocketFin fin,
            WebSocketRsv rsv1,
            WebSocketRsv rsv2,
            WebSocketRsv rsv3,
            WebSocketOpcode opcode,
            WebSocketMask mask,
            byte[] maskingKey,
            byte payloadLength,
            byte[] extPayloadLength,
            Stream payload
        )
        {
            if (mask == WebSocketMask.Mask && (maskingKey == null || maskingKey.Length != 4))
                throw new ArgumentOutOfRangeException("maskingKey", "must by 4 bytes long");

            _fin = fin;
            _rsv1 = rsv1;
            _rsv2 = rsv2;
            _rsv3 = rsv3;
            _opcode = opcode;
            _mask = mask;
            _maskingKey = maskingKey;
            _payloadLength = payloadLength;
            _extPayloadLength = extPayloadLength;
            _payload = payload;
        }

        public WebSocketFrame(
            WebSocketFin fin,
            WebSocketRsv rsv1,
            WebSocketRsv rsv2,
            WebSocketRsv rsv3,
            WebSocketOpcode opcode,
            WebSocketMask mask,
            byte[] maskingKey,
            Stream payload
        )
            : this(fin, rsv1, rsv2, rsv3, opcode, mask, maskingKey, 0, new byte[0], payload)
        {
            if (payload != null)
            {
                var len = payload.Length;
                if (len < 126)
                {
                    _payloadLength = (byte)len;
                    _extPayloadLength = new byte[0];
                }
                else if (len < 0x010000)
                {
                    _payloadLength = (byte)126;
                    _extPayloadLength = WebSocketUtils.GetBigEndianBytes((ushort)len);
                }
                else
                {
                    _payloadLength = (byte)127;
                    _extPayloadLength = WebSocketUtils.GetBigEndianBytes((ulong)len);
                }
            }
        }

        public WebSocketFrame(
            WebSocketFin fin,
            WebSocketOpcode opcode,
            WebSocketMask mask,
            Stream payload
        )
            : this(fin, WebSocketRsv.Off, WebSocketRsv.Off, WebSocketRsv.Off, opcode, mask, mask == WebSocketMask.Mask ? WebSocketUtils.CreateMaskingKey() : new byte[0], payload)
        {

        }

        /// <summary>
        /// Final frame or not
        /// </summary>
        public WebSocketFin Fin
        {
            get
            {
                return _fin;
            }
        }

        /// <summary>
        /// Extension switch one
        /// </summary>
        public WebSocketRsv Rsv1
        {
            get
            {
                return _rsv1;
            }
        }

        /// <summary>
        /// Extension switch two
        /// </summary>
        public WebSocketRsv Rsv2
        {
            get
            {
                return _rsv2;
            }
        }

        /// <summary>
        /// Extension switch three
        /// </summary>
        public WebSocketRsv Rsv3
        {
            get
            {
                return _rsv3;
            }
        }

        /// <summary>
        /// Type of frame
        /// </summary>
        public WebSocketOpcode Opcode
        {
            get
            {
                return _opcode;
            }
        }

        /// <summary>
        /// Is frame masked
        /// </summary>
        public WebSocketMask Mask
        {
            get
            {
                return _mask;
            }
        }

        /// <summary>
        /// Masking key
        /// </summary>
        public byte[] MaskingKey
        {
            get
            {
                return _maskingKey;
            }
        }

        /// <summary>
        /// Payload of the frame
        /// </summary>
        public Stream Payload
        {
            get
            {
                return _payload;
            }
        }

        /// <summary>
        /// Payload length
        /// </summary>
        public byte PayloadLength
        {
            get
            {
                return _payloadLength;
            }
        }

        /// <summary>
        /// Extended payload length
        /// </summary>
        public byte[] ExtPayloadLength
        {
            get
            {
                return _extPayloadLength;
            }
        }

        /// <summary>
        /// Applies the current masking key on the payload
        /// </summary>
        public void Unmask()
        {
            if (Mask == WebSocketMask.Unmask)
                return;

            _mask = WebSocketMask.Unmask;
            if (Payload != null)
            {
                using (Payload)
                {
                    var unmasked = new MemoryStream((int)Payload.Length);
                    Payload.Position = 0;
                    for (var i = 0; i < Payload.Length; i++)
                    {
                        unmasked.WriteByte((byte)(Payload.ReadByte() ^ MaskingKey[i % 4]));
                    }
                    unmasked.Position = 0;
                    _payload = unmasked;
                }
            }
            _maskingKey = new byte[0];
        }

    }
}
