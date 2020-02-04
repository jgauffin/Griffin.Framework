using System.Threading.Tasks;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Messaging;

namespace Griffin.Net.Authentication
{
    /// <summary>
    ///     Used to authenticate in the client
    /// </summary>
    public interface IClientAuthenticator
    {
        /// <summary>
        ///     Determins if this client requires raw byte data to be able to authenticate
        /// </summary>
        bool RequiresRawData { get; }

        /// <summary>
        ///     If we've failed to authenticate (no need to try again unless credentials are changed)
        /// </summary>
        bool AuthenticationFailed { get; }

        /// <summary>
        ///     authenticate using serialized messages
        /// </summary>
        /// <param name="channel">channel to authenticate</param>
        /// <param name="message">Received message, will be <see cref="AuthenticationRequiredException"/> first time and then responses to your authentication messages</param>
        /// <returns><c>true</c> if authentication process completed.</returns>
        Task<bool> ProcessAsync(IMessagingChannel channel, object message);
    }
}