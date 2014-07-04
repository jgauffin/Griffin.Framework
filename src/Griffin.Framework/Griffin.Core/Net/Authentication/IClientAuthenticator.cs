using Griffin.Net.Channels;

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
        bool Process(ITcpChannel channel, object message);

        /// <summary>
        ///     Process raw bytes.
        /// </summary>
        /// <param name="channel">Channel that the bytes came from</param>
        /// <param name="buffer">Buffer to process</param>
        /// <param name="completed">Authentication process have completed, we either failed or succeeded</param>
        /// <returns>Number of bytes processed from the buffer.</returns>
        int Process(ITcpChannel channel, ISocketBuffer buffer, out bool completed);
    }
}