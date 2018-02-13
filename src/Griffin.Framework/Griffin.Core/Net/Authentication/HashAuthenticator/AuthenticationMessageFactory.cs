using System;
using Griffin.Net.Authentication.Messages;
using Griffin.Security;

namespace Griffin.Net.Authentication.HashAuthenticator
{
    /// <summary>
    ///     Default implementation which creates the default messages defined in the [Messages](Messages) folder.
    /// </summary>
    public class AuthenticationMessageFactory : IAuthenticationMessageFactory
    {
        private IPasswordHasher _hasher = new PasswordHasherRfc2898();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMessageFactory"/> class.
        /// </summary>
        public AuthenticationMessageFactory()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMessageFactory"/> class.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <exception cref="System.ArgumentNullException">hasher</exception>
        public AuthenticationMessageFactory(IPasswordHasher hasher)
        {
            if (hasher == null) throw new ArgumentNullException("hasher");
            Hasher = hasher;
        }

        /// <summary>
        /// Used by the factory to generate salts.
        /// </summary>
        public IPasswordHasher Hasher
        {
            get { return _hasher; }
            set { _hasher = value; }
        }

        /// <summary>
        ///     Create the first message (sent from the client to the server once connected)
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        ///     <see cref="AuthenticationHandshake" />
        /// </returns>
        public IAuthenticationHandshake CreateHandshake(string userName)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            return new AuthenticationHandshake {UserName = userName};
        }

        /// <summary>
        ///     Create the message that are sent from the server to the client with the salts that should be used during the
        ///     authentication.
        /// </summary>
        /// <param name="user">User that want to be authenticated</param>
        /// <returns>
        ///     <see cref="AuthenticationHandshakeReply" />
        /// </returns>
        public IAuthenticationHandshakeReply CreateServerPreAuthentication(IUserAccount user)
        {
            return new AuthenticationHandshakeReply()
            {
                AccountSalt = user.PasswordSalt,
                SessionSalt = Hasher.CreateSalt()
            };
        }

        /// <summary>
        ///     The last authentication step. Contains the hashed authentication string
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        ///     <see cref="Authenticate" />
        /// </returns>
        public IAuthenticate CreateAuthentication(string token)
        {
            return new Authenticate
            {
                AuthenticationToken = token,
                ClientSalt = Hasher.CreateSalt()
            };
        }

        /// <summary>
        ///     Sent by server to indicate how the authentication went
        /// </summary>
        /// <param name="state">How it went</param>
        /// <param name="authenticationToken"></param>
        /// <returns>
        ///     <see cref="AuthenticateReply" />
        /// </returns>
        public IAuthenticateReply CreateAuthenticationResult(AuthenticateReplyState state, string authenticationToken)
        {
            if (authenticationToken == null) throw new ArgumentNullException("authenticationToken");
            return new AuthenticateReply {State = state, AuthenticationToken = authenticationToken};
        }
    }
}