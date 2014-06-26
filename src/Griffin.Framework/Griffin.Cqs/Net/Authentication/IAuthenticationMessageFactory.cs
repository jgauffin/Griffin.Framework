using Griffin.Cqs.Net.Authentication.Messages;

namespace Griffin.Cqs.Net.Authentication
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
        /// <returns>message</returns>
        IClientPreAuthentication CreateClientPreAuthentication();

        /// <summary>
        ///     Create the message that are sent from the server to the client with the salts that should be used during the
        ///     authentication.
        /// </summary>
        /// <param name="user">User that want to be authenticated</param>
        /// <returns>message</returns>
        IServerPreAuthentication CreateServerPreAuthentication(IUserAccount user);

        /// <summary>
        ///     The last authentication step. Contains the hashed authentication string
        /// </summary>
        /// <returns>message</returns>
        IClientAuthentication CreateClientAuthentication();
    }
}