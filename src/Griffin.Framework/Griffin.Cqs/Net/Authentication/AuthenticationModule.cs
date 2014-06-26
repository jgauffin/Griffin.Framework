using System;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Cqs.Net.Authentication.Messages;
using Griffin.Net.Server.Modules;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Used to authenticate users.
    /// </summary>
    public class AuthenticationModule : IServerModule
    {
        private readonly IUserFetcher _fetcher;
        private IAuthenticationMessageFactory _authenticationMessageFactory;
        private IPasswordHasher _passwordHasher = new PasswordHasher();
        private IPrincipalFactory _principalFactory;

        public AuthenticationModule(IUserFetcher fetcher)
        {
            _fetcher = fetcher;
            PrincipalFactory = new GenericPrincipalFactory(fetcher);
        }

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
        public async Task BeginRequestAsync(IClientContext context)
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
        public async Task EndRequest(IClientContext context)
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

            var user = context.ChannelData["AuthenticationUser"] as IUserAccount;
            if (user == null)
            {
                var preRequest = context.RequestMessage as IClientPreAuthentication;
                if (preRequest == null)
                {
                    context.ResponseMessage =
                        new AuthenticationException(
                            "You need to send a message that implements IClientPreAuthentication.");
                    return ModuleResult.SendResponse;
                }

                user = await _fetcher.FindUserAsync(preRequest.UserName);
                var msg = _authenticationMessageFactory.CreateServerPreAuthentication(user);
                context.ResponseMessage = msg;
                context.ChannelData["SessionSalt"] = msg.SessionSalt;
                context.ChannelData["AuthenticationUser"] = user;
                return ModuleResult.SendResponse;
            }
            else
            {
                var authenticateRequest = context.RequestMessage as IClientAuthentication;
                if (authenticateRequest == null)
                {
                    context.ResponseMessage =
                        new AuthenticationException(
                            "You need to send a message that implements IClientAuthentication as the second message to authenticate properly.");
                    return ModuleResult.SendResponse;
                }

                var sessionSalt = context.ChannelData["SessionSalt"] as string;
                if (sessionSalt == null)
                {
                    context.ResponseMessage =
                        new AuthenticationException(
                            "Invalid authentication process (salt not found)..");
                    return ModuleResult.SendResponse;
                }

                var serverHash = _passwordHasher.HashPassword(user.HashedPassword, sessionSalt);
                if (_passwordHasher.Compare(serverHash, authenticateRequest.AuthenticationToken))
                {
                    context.ChannelData["Principal"] = await PrincipalFactory.CreatePrincipalAsync(user);
                }

                var msg = _authenticationMessageFactory.CreateServerPreAuthentication(user);
                context.ResponseMessage = msg;
                return ModuleResult.SendResponse;
            }
        }
    }
}