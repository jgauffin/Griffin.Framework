using System;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading.Tasks;
using Griffin.Net.Authentication.HashAuthenticator;
using Griffin.Net.Authentication.Messages;
using Griffin.Security;
using Griffin.Security.Authentication;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    /// <summary>
    ///     Used to authenticate users.
    /// </summary>
    public class HashAuthenticationMiddleware : MicroMessageMiddleware
    {
        private readonly IAccountRepository _accountRepository;
        private IPasswordHasher _passwordHasher = new PasswordHasherRfc2898();
        private IPrincipalFactory _principalFactory;
        private int _step;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashAuthenticationMiddleware" /> class.
        /// </summary>
        /// <param name="accountRepository">The fetcher.</param>
        public HashAuthenticationMiddleware(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
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
            get => _passwordHasher;
            set => _passwordHasher = value ?? throw new ArgumentNullException("value");
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
            get => _principalFactory;
            set => _principalFactory = value ?? throw new ArgumentNullException("value");
        }

        public override async Task Process(MicroMessageContext context, Func<Task> next)
        {
            if (context.User != null)
            {
                await next();
                return;
            }

            if (context.RequestMessage is IAuthenticationHandshake handshake)
            {
                await ProcessHandshake(context, handshake);
                return;
            }

            var authenticateRequest = context.RequestMessage as IAuthenticate;
            if (_step == 0 || authenticateRequest == null)
            {
                await next();
                return;
            }

            var user = context.ChannelData["AuthenticationUser"] as UserAccount;
            if (user == null || _step != 1)
            {
                context.Response = new AuthenticationException("Invalid authentication process");
                _step = 0;
                return;
            }

            _step = 0;
            if (context.ChannelData["SessionSalt"] is string sessionSalt)
            {
                var response = await AuthenticateUser(context, user, sessionSalt, authenticateRequest);
                context.Response = response;
                return;
            }

            context.Response =
                new AuthenticationException(
                    "Invalid authentication process (salt not found)..");
        }

        private async Task ProcessHandshake(MicroMessageContext context, IAuthenticationHandshake handshake)
        {
            if (_step != 0)
            {
                context.Response =
                    new AuthenticationException(
                        "Invalid authentication process.");
                return;
            }

            var user = await _accountRepository.FindByUserName(handshake.UserName);
            var msg = new AuthenticationHandshakeReply()
            {
                AccountSalt = user.PasswordSalt,
                SessionSalt = PasswordHasher.CreateSalt()
            };

            context.Response = msg;
            context.ChannelData["SessionSalt"] = msg.SessionSalt;
            context.ChannelData["AuthenticationUser"] = user;
            _step = 1;
        }

        private async Task<object> AuthenticateUser(MicroMessageContext context, UserAccount user, string sessionSalt,
            IAuthenticate authenticateRequest)
        {
            object response;
            if (user.IsLocked)
            {
                response = new AuthenticateReply { State = AuthenticateReplyState.Locked };
            }
            else
            {
                var serverHash = _passwordHasher.HashPassword(user.HashedPassword, sessionSalt);
                if (_passwordHasher.Compare(serverHash, authenticateRequest.AuthenticationToken))
                {
                    context.User = await PrincipalFactory.CreatePrincipalAsync(user);
                    var proof = _passwordHasher.HashPassword(user.HashedPassword, authenticateRequest.ClientSalt);
                    response = new AuthenticateReply { State = AuthenticateReplyState.Locked, AuthenticationToken = proof };
                }
                else
                {
                    response = new AuthenticateReply { State = AuthenticateReplyState.IncorrectLogin };
                }
            }

            return response;
        }
    }
}