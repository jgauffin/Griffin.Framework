using System;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Net.Authentication;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Authentication.Messages;
using Griffin.Security;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    /// <summary>
    ///     Used to authenticate users.
    /// </summary>
    public class HashAuthenticationModule : IServerModule
    {
        private readonly IUserFetcher _fetcher;
        private IAuthenticationMessageFactory _authenticationMessageFactory;
        private IPasswordHasher _passwordHasher = new PasswordHasherRfc2898();
        private IPrincipalFactory _principalFactory;
        private int _step = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashAuthenticationModule" /> class.
        /// </summary>
        /// <param name="fetcher">The fetcher.</param>
        public HashAuthenticationModule(IUserFetcher fetcher)
        {
            if (fetcher == null) throw new ArgumentNullException("fetcher");

            _fetcher = fetcher;
            PrincipalFactory = new GenericPrincipalFactory(fetcher);
            _authenticationMessageFactory = new AuthenticationMessageFactory();
        }

        /// <summary>
        ///     Used to produce the messages that are sent betweent the client/server during the authentication process.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can define your own factory if the default messages do not work with your serializer.
        ///     </para>
        /// </remarks>
        public IAuthenticationMessageFactory AuthenticationMessageFactory
        {
            get { return _authenticationMessageFactory; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _authenticationMessageFactory = value;
            }
        }

        /// <summary>
        ///     Used to hash the passwords with the session salt (and at the client side to hash the password entered by the user).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can replace the password hasher if your password is not stored using our hash format.
        ///     </para>
        /// </remarks>
        public IPasswordHasher PasswordHasher
        {
            get { return _passwordHasher; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _passwordHasher = value;
            }
        }

        /// <summary>
        ///     Used to load the correct <c>IPrincipal</c> implementation once the user have been registered.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Use your own factory if you do not want to use <see cref="GenericPrincipal" />.
        ///     </para>
        /// </remarks>
        public IPrincipalFactory PrincipalFactory
        {
            get { return _principalFactory; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _principalFactory = value;
            }
        }

        /// <summary>
        ///     Begin request is always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
#pragma warning disable 1998
        public async Task BeginRequestAsync(IClientContext context)
#pragma warning restore 1998
        {
        }

        /// <summary>
        ///     Always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <remarks>
        ///     <para>
        ///         Check the <see cref="ModuleResult" /> property to see how the message processing have gone so
        ///         far.
        ///     </para>
        /// </remarks>
#pragma warning disable 1998
        public async Task EndRequest(IClientContext context)
#pragma warning restore 1998
        {
        }

        /// <summary>
        ///     ProcessAsync message
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        /// <remarks>
        ///     <para>
        ///         Check the <see cref="ModuleResult" /> property to see how the message processing have gone so
        ///         far.
        ///     </para>
        /// </remarks>
        public async Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            var principal = context.ChannelData["Principal"] as IPrincipal;
            if (principal != null)
            {
                Thread.CurrentPrincipal = principal;
                return ModuleResult.Continue;
            }

            var handshake = context.RequestMessage as IAuthenticationHandshake;
            if (handshake != null)
            {
                if (_step != 0)
                {
                    context.ResponseMessage =
                        new AuthenticationException(
                            "Invalid authentication process.");
                    return ModuleResult.SendResponse;
                }

                var user = await _fetcher.FindUserAsync(handshake.UserName);
                var msg = _authenticationMessageFactory.CreateServerPreAuthentication(user);
                context.ResponseMessage = msg;
                context.ChannelData["SessionSalt"] = msg.SessionSalt;
                context.ChannelData["AuthenticationUser"] = user;
                _step = 1;
                return ModuleResult.SendResponse;
            }
            if (_step == 0)
                return ModuleResult.Continue;

            var authenticateRequest = context.RequestMessage as IAuthenticate;
            if (authenticateRequest != null)
            {
                var user = context.ChannelData["AuthenticationUser"] as IUserAccount;
                if (user == null || _step != 1)
                {
                    context.ResponseMessage = new AuthenticationException("Invalid authentication process");
                    _step = 0;
                    return ModuleResult.SendResponse;
                }
                _step = 0;

                var sessionSalt = context.ChannelData["SessionSalt"] as string;
                if (sessionSalt == null)
                {
                    context.ResponseMessage =
                        new AuthenticationException(
                            "Invalid authentication process (salt not found)..");
                    return ModuleResult.SendResponse;
                }


                var response = await AuthenticateUser(context, user, sessionSalt, authenticateRequest);
                context.ResponseMessage = response;
                return ModuleResult.SendResponse;
            }

            return ModuleResult.Continue;
        }

        private async Task<object> AuthenticateUser(IClientContext context, IUserAccount user, string sessionSalt,
            IAuthenticate authenticateRequest)
        {
            object response;
            if (user.IsLocked)
                response = _authenticationMessageFactory.CreateAuthenticationResult(AuthenticateReplyState.Locked, null);
            else
            {
                var serverHash = _passwordHasher.HashPassword(user.HashedPassword, sessionSalt);
                if (_passwordHasher.Compare(serverHash, authenticateRequest.AuthenticationToken))
                {
                    context.ChannelData["Principal"] = await PrincipalFactory.CreatePrincipalAsync(user);
                    var proof = _passwordHasher.HashPassword(user.HashedPassword, authenticateRequest.ClientSalt);
                    response = _authenticationMessageFactory.CreateAuthenticationResult(AuthenticateReplyState.Success,
                        proof);
                }
                else
                    response =
                        _authenticationMessageFactory.CreateAuthenticationResult(AuthenticateReplyState.IncorrectLogin,
                            null);
            }
            return response;
        }
    }
}