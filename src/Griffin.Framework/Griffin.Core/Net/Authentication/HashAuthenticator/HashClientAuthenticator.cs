using System;
using System.Net;
using System.Threading.Tasks;
using Griffin.Net.Authentication.Messages;
using Griffin.Net.Channels;
using Griffin.Net.Messaging;
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
    ///         If you are using something else than our own hashing algorithm (<see cref="PasswordHasherRfc2898" />) for
    ///         storing passwords you need to set your own
    ///         algorithm using <see cref="ConfigureAuthentication" />.
    ///     </para>
    /// </remarks>
    public class HashClientAuthenticator : IClientAuthenticator
    {
        private string _clientSalt;
        private NetworkCredential _credentials;
        private string _passwordHash;
        private IPasswordHasher _passwordHasher = new PasswordHasherRfc2898();
        private int _state;

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
            Credential = credential ?? throw new ArgumentNullException("credential");
        }

        /// <summary>
        ///     Use this method if you want to use the built in authentication library.
        /// </summary>
        public NetworkCredential Credential
        {
            get => _credentials;
            set
            {
                AuthenticationFailed = false;
                _credentials = value ?? throw new ArgumentNullException("value");
            }
        }

        /// <summary>
        ///     Will always return <c>false</c> as this implementation only works with serialized messages.
        /// </summary>
        public bool RequiresRawData => false;

        /// <summary>
        ///     Wether we have failed to authenticate
        /// </summary>
        public bool AuthenticationFailed { get; set; }


        /// <summary>
        ///     authenticate using serialized messages
        /// </summary>
        /// <param name="channel">channel to authenticate</param>
        /// <param name="message">
        ///     Received message, will be <see cref="AuthenticationRequiredException" /> first time and then
        ///     responses to your authentication messages
        /// </param>
        /// <returns>
        ///     <c>true</c> if authentication process completed.
        /// </returns>
        public async Task<bool> ProcessAsync(IMessagingChannel channel, object message)
        {
            if (message is AuthenticationRequiredException)
            {
                var handshake = new AuthenticationHandshake { UserName = _credentials.UserName };
                await channel.SendAsync(handshake);
                _state = 1;
                AuthenticationFailed = false;
                return false;
            }

            if (message is AuthenticationHandshakeReply handshakeReply)
            {
                if (_state != 1)
                {
                    AuthenticationFailed = true;
                    return true;
                }

                _state = 2;
                _passwordHash = _passwordHasher.HashPassword(_credentials.Password, handshakeReply.AccountSalt);
                var token = _passwordHasher.HashPassword(_passwordHash, handshakeReply.SessionSalt);
                var auth = new Authenticate
                {
                    AuthenticationToken = token,
                    ClientSalt = _passwordHasher.CreateSalt()
                };

                _clientSalt = auth.ClientSalt;
                await channel.SendAsync(auth);
                return false;
            }

            if (message is AuthenticateReply result)
            {
                if (_state != 2)
                {
                    AuthenticationFailed = true;
                    return true;
                }

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
        ///     Used to configure how passwords are being hashed and which messages should be used when transferring authentication
        ///     information
        ///     over the network.
        /// </summary>
        /// <param name="hasher">Password hasher, the default one is <see cref="PasswordHasherRfc2898" />.</param>
        public void ConfigureAuthentication(IPasswordHasher hasher)
        {
            _passwordHasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }
    }
}