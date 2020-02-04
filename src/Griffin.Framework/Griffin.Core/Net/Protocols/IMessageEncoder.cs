using System.Net.Sockets;
using System.Threading.Tasks;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols
{
    /// <summary>
    ///     Message encoders are used to convert objects into binary form so that they can be transferred over a socket.
    /// </summary>
    /// <remarks>
    ///     The format itself is determined by the protocol which is implemented. See all implementations.
    /// </remarks>
    public interface IMessageEncoder
    {
        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="message">Message to encode</param>
        /// <param name="channel">Channel used to transfer the encoded data</param>
        /// <remarks>
        ///     The <c>buffer</c> variable is typically a wrapper around <see cref="SocketAsyncEventArgs" />, but may be something
        ///     else if required.
        /// </remarks>
        Task EncodeAsync(object message, IBinaryChannel channel);

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        void Clear();
    }
}