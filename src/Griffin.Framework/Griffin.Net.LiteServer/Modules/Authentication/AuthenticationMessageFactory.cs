using System;
using Griffin.Net.LiteServer.Modules.Authentication.Messages;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    /// <summary>
    ///     Default implementation which creates the default messages defined in the [Messages](Messages) folder.
    /// </summary>
    public class AuthenticationMessageFactory : IAuthenticationMessageFactory
    {
        private PasswordHasher _hasher = new PasswordHasher();

        /// <summary>
        ///     Create the first message (sent from the client to the server once connected)
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        ///     <see cref="ClientPreAuthentication" />
        /// </returns>
        public IClientPreAuthentication CreateClientPreAuthentication(string userName)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            return new ClientPreAuthentication {UserName = userName};
        }

        /// <summary>
        ///     Create the message that are sent from the server to the client with the salts that should be used during the
        ///     authentication.
        /// </summary>
        /// <param name="user">User that want to be authenticated</param>
        /// <returns>
        ///     <see cref="ServerPreAuthentication" />
        /// </returns>
        public IServerPreAuthentication CreateServerPreAuthentication(IUserAccount user)
        {
            return new ServerPreAuthentication()
            {
                AccountSalt = user.PasswordSalt,
                SessionSalt = _hasher.CreateSalt()
            };
        }

        /// <summary>
        ///     The last authentication step. Contains the hashed authentication string
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        ///     <see cref="ClientAuthentication" />
        /// </returns>
        public IClientAuthentication CreateClientAuthentication(string token)
        {
            return new ClientAuthentication
            {
                AuthenticationToken = token
            };
        }

        /// <summary>
        ///     Sent by server to indicate how the authentication went
        /// </summary>
        /// <param name="state">How it went</param>
        /// <returns>
        ///     <see cref="AuthenticationResult" />
        /// </returns>
        public IAuthenticationResult CreateAuthenticationResult(AuthenticationResultState state)
        {
            return new AuthenticationResult {State = state};
        }
    }
}