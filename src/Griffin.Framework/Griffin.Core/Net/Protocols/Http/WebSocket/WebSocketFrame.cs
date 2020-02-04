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
    public class WebSocketFrame
    {
        /// <summary>
        /// Maximum fragment length. (<value>65535</value>)
        /// Can be up to UInt64.MaxValue
        /// </summary>
        public const int FragmentLength = 65535; // 1016;

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

            Fin = fin;
            Rsv1 = rsv1;
            Rsv2 = rsv2;
            Rsv3 = rsv3;
            Opcode = opcode;
            Mask = mask;
            MaskingKey = maskingKey;
            PayloadLength = payloadLength;
            ExtPayloadLength = extPayloadLength;
            Payload = payload;
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
                    PayloadLength = (byte)len;
                    ExtPayloadLength = new byte[0];
                }
                else if (len < 0x010000)
                {
                    PayloadLength = (byte)126;
                    ExtPayloadLength = WebSocketUtils.GetBigEndianBytes((ushort)len);
                }
                else
                {
                    PayloadLength = (byte)127;
                    ExtPayloadLength = WebSocketUtils.GetBigEndianBytes((ulong)len);
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
        public WebSocketFin Fin { get; }

        /// <summary>
        /// Extension switch one
        /// </summary>
        public WebSocketRsv Rsv1 { get; }

        /// <summary>
        /// Extension switch two
        /// </summary>
        public WebSocketRsv Rsv2 { get; }

        /// <summary>
        /// Extension switch three
        /// </summary>
        public WebSocketRsv Rsv3 { get; }

        /// <summary>
        /// Type of frame
        /// </summary>
        public WebSocketOpcode Opcode { get; }

        /// <summary>
        /// Is frame masked
        /// </summary>
        public WebSocketMask Mask { get; private set; }

        /// <summary>
        /// Masking key
        /// </summary>
        public byte[] MaskingKey { get; private set; }

        /// <summary>
        /// Payload of the frame
        /// </summary>
        public Stream Payload { get; private set; }

        /// <summary>
        /// Payload length
        /// </summary>
        public byte PayloadLength { get; }

        /// <summary>
        /// Extended payload length
        /// </summary>
        public byte[] ExtPayloadLength { get; }

        /// <summary>
        /// Applies the current masking key on the payload
        /// </summary>
        public void Unmask()
        {
            if (Mask == WebSocketMask.Unmask)
                return;

            Mask = WebSocketMask.Unmask;
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
                    Payload = unmasked;
                }
            }
            MaskingKey = new byte[0];
        }

    }
}
