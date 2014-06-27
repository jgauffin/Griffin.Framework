using Griffin.Security;

namespace Griffin.Net.Authentication.Messages
{
    /// <summary>
    ///     Used to create the authentication messages which are sent between the client/server.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Some serializer require that messages have been tagged with attributes or are constructed in a certain way.
    ///         This contract allows
    ///         you to create messages that works with your currently selected message serializer.
    ///     </para>
    /// </remarks>
    public interface IAuthenticationMessageFactory
    {
        /// <summary>
        ///     Create the first message (sent from the client to the server once connected)
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        ///     message that can be serialized
        /// </returns>
        IAuthenticationHandshake CreateHandshake(string userName);

        /// <summary>
        ///     Create the message that are sent from the server to the client with the salts that should be used during the
        ///     authentication.
        /// </summary>
        /// <param name="user">User that want to be authenticated</param>
        /// <returns>message that can be serialized</returns>
        /// <remarks>
        /// <para>You are responsible of generating the session salt. You can for instance do that by using <see cref="PasswordHasherRfc2898.CreateSalt"/>.</para>
        /// </remarks>
        IAuthenticationHandshakeReply CreateServerPreAuthentication(IUserAccount user);

        /// <summary>
        ///     The last authentication step. Contains the hashed authentication string
        /// </summary>
        /// <returns>message that can be serialized</returns>
        IAuthenticate CreateAuthentication(string token);

        /// <summary>
        ///     Sent by server to indicate how the authentication went
        /// </summary>
        /// <param name="state">How it went</param>
        /// <param name="authenticationToken">Token created by the server using <see cref="IAuthenticate.ClientSalt"/> to prove the server identity. <c>null</c> if authentication failed.</param>
        /// <returns>message that can be serialized</returns>
        IAuthenticateReply CreateAuthenticationResult(AuthenticateReplyState state, string authenticationToken);
    }
}