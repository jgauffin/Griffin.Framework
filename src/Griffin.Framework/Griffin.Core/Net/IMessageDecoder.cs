using System;
using Griffin.Net.Channels;

namespace Griffin.Net
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageDecoder
    {

        /// <summary>
        /// We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        void ProcessReadBytes(ISocketBuffer buffer);

        void Clear();

        /// <summary>
        /// A message have been received.
        /// </summary>
        /// <remarks>
        /// Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        Action<object> MessageReceived { get; set; }
    }
}