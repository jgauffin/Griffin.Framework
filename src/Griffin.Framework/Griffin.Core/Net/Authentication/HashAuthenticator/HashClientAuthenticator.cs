using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Griffin.Net.Authentication.Messages;
using Griffin.Net.Channels;
using Griffin.Security;

namespace Griffin.Net.Authentication.HashAuthenticator
{
    /// <summary>
    ///     Authenticate using the built in mechanism in Griffin.Framework. requires that the lite server is using the
    ///     <c>HashAuthenticationModule</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To make this work you need to have stored passwords in your database by hashing them with a salt. The hash must
    ///         have been
    ///         created using <c>Rfc2898DeriveBytes</c> with 1000 iterations upon the string "password:hash". You you are using
    ///         another
    ///         way of storing passwords in your DB you need to configure the authentication process using
    ///         <see cref="ConfigureAuthentication" />.
    ///     </para>
    ///     <para>
    ///         If you are using something else than our own hashing algortihm (<see cref="PasswordHasherRfc2898" />) for
    ///         storing passwords you need to set your own
    ///         algortihm using <see cref="ConfigureAuthentication" />.
    ///     </para>
    /// </remarks>
    public class HashClientAuthenticator : IClientAuthenticator
    {
        private NetworkCredential _credentials;
        private IAuthenticationMessageFactory _authenticationMessageFactory =
            new AuthenticationMessageFactory(new PasswordHasherRfc2898());
        private IPasswordHasher _passwordHasher = new PasswordHasherRfc2898();
        private int _state;
        private string _passwordHash;
        private string _clientSalt;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashClientAuthenticator" /> class.
        /// </summary>
        /// <param name="credential">The credential to login with.</param>
        /// <exception cref="System.ArgumentNullException">credential</exception>
        /// <remarks>
        ///     <para>See class documentation for more information about how this client works</para>
        /// </remarks>
        public HashClientAuthenticator(NetworkCredential credential)
        {
            if (credential == null) throw new ArgumentNullException("credential");
            Credential = credential;
        }

        /// <summary>
        ///     Use this method if you want to use the built in authentication library.
        /// </summary>
        public NetworkCredential Credential
        {
            get { return _credentials; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                AuthenticationFailed = false;
                _credentials = value;
            }
        }

        /// <summary>
        ///     Will always return <c>false</c> as this implementation only works with serialized messages.
        /// </summary>
        public bool RequiresRawData
        {
            get { return false; }
        }

        /// <summary>
        ///     Wether we have failed to authenticate
        /// </summary>
        public bool AuthenticationFailed { get; set; }


        /// <summary>
        /// authenticate using serialized messages
        /// </summary>
        /// <param name="channel">channel to authenticate</param>
        /// <param name="message">Received message, will be <see cref="AuthenticationRequiredException" /> first time and then responses to your authentication messages</param>
        /// <returns>
        ///   <c>true</c> if authentication process completed.
        /// </returns>
        public bool Process(ITcpChannel channel, object message)
        {
            if (message is AuthenticationRequiredException)
            {
                var handshake = _authenticationMessageFactory.CreateHandshake(_credentials.UserName);
                channel.Send(handshake);
                _state = 1;
                AuthenticationFailed = false;
                return false;
            }

            if (message is AuthenticationHandshakeReply)
            {
                if (_state != 1)
                {
                    AuthenticationFailed = true;
                    return true;
                }

                _state = 2;
                var handshakeReply = (AuthenticationHandshakeReply)message;
                _passwordHash = _passwordHasher.HashPassword(_credentials.Password, handshakeReply.AccountSalt);
                var token = _passwordHasher.HashPassword(_passwordHash, handshakeReply.SessionSalt);
                var auth = _authenticationMessageFactory.CreateAuthentication(token);
                _clientSalt = auth.ClientSalt;
                channel.Send(auth);
                return false;
            }

            if (message is AuthenticateReply)
            {
                if (_state != 2)
                {
                    AuthenticationFailed = true;
                    return true;
                }

                var result = (AuthenticateReply)message;
                AuthenticationFailed = true;
                if (result.State == AuthenticateReplyState.Success)
                {
                    var ourToken = _passwordHasher.HashPassword(_passwordHash, _clientSalt);
                    if (!_passwordHasher.Compare(ourToken, result.AuthenticationToken))
                        return true;

                }
                else
                    return true;

                AuthenticationFailed = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="buffer"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public int Process(ITcpChannel channel, ISocketBuffer buffer, out bool completed)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Used to configure how passwords are being hashed and which messages should be used when transfering authentication
        ///     information
        ///     over the network.
        /// </summary>
        /// <param name="hasher">Password hasher, the dault one is <see cref="PasswordHasherRfc2898" />.</param>
        /// <param name="messageFactory">Message factory, the default on is <see cref="AuthenticationMessageFactory" />.</param>
        public void ConfigureAuthentication(IPasswordHasher hasher, IAuthenticationMessageFactory messageFactory)
        {
            if (hasher == null) throw new ArgumentNullException("hasher");
            if (messageFactory == null) throw new ArgumentNullException("messageFactory");
            _authenticationMessageFactory = messageFactory;
            _passwordHasher = hasher;
        }

    }
}