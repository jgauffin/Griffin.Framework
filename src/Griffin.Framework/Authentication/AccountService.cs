using System;
using System.Security.Principal;
using System.Threading;
using Griffin.Framework.Authentication.Password;
using Griffin.Framework.Authentication.Password.Strategies;
using Griffin.Framework.Authentication.Password.Validation;
using Griffin.Framework.Authentication.Principals;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Default implementation of <see cref="IAccountService"/>
    /// </summary>
    public class AccountService : IAccountService
    {
        private const int MaxPasswordAttempts = 5;
        private readonly IAccountStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService" /> class.
        /// </summary>
        /// <param name="storage">Where items should have be stored.</param>
        public AccountService(IAccountStorage storage)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            _storage = storage;
            PasswordStrategy = new HashPasswordStrategy();
            PasswordValidator = new MinLengthValidator(10);
            PrincipalFactory = new GenericPrincipalFactory(new NoRolesStorage());
        }

        /// <summary>
        /// Gets or sets which password strategy to use
        /// </summary>
        public IPasswordStrategy PasswordStrategy { get; set; }

        /// <summary>
        /// Gets or sets the kind of validation that should be done for passwords
        /// </summary>
        /// <remarks>Use the <see cref="CompositeValidation"/> class if you want to use multiple validations</remarks>
        public IPasswordValidator PasswordValidator { get; set; }

        /// <summary>
        /// Gets or sets principal factory
        /// </summary>
        public IPrincipalFactory PrincipalFactory { get; set; }

        /// <summary>
        /// Gets or sets event subscribers
        /// </summary>
        public IAccountServiceEvents[] Subscribers { get; set; }


        #region IAccountService Members

        /// <summary>
        /// Gets or sets if all new accounts have to be activated using emails.
        /// </summary>
        /// <remarks>You are yourself responsible of sending out the email. The created account will have an activation code after that the <see cref="Register"/> method has been invoked.</remarks>
        public bool UseActivation { get; set; }

        /// <summary>
        /// Will login without any checks.
        /// </summary>
        /// <param name="accountName">Account to log into.</param>
        /// <param name="account">Loaded account</param>
        public bool TryLogin(string accountName, out Account account)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            account = _storage.LoadByAccountName(accountName);
            if (account == null)
                throw new InvalidOperationException("Account not found: " + accountName);


            var result = account.Login();
            _storage.Store(account);
            if (result == LoginResult.Locked)
                throw new AccountLockedException(accountName);
            if (result != LoginResult.Success)
                return false;

            Thread.CurrentPrincipal = PrincipalFactory.CreatePrincipal(account);

            var k = account;
            TriggerEvent(x => x.LoggedIn(k.Id, k.AccountName, k.Email));
            return false;
        }

        /// <summary>
        /// Standard login
        /// </summary>
        /// <param name="accountName">Account to log into.</param>
        /// <param name="password">Password</param>
        /// <param name="account">Account which was logged in</param>
        /// <returns>
        ///   <c>true</c> if successful; otherwise <c>false</c>
        /// </returns>
        /// <seealso cref="IPrincipalFactory" />
        /// <exception cref="System.ArgumentNullException">accountName</exception>
        /// <remarks>
        /// Will assign the <c>Thread.CurrentPrincipal</c> with the corrent principal if login was successful (using the <see cref="IPrincipalFactory" />).
        /// </remarks>
        public bool TryLogin(string accountName, string password, out Account account)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            if (password == null) throw new ArgumentNullException("password");

            account = _storage.LoadByAccountName(accountName);
            if (account == null)
                return false;

            var result = account.TryLogin(PasswordStrategy, MaxPasswordAttempts, password);
            _storage.Store(account);

            if (result == LoginResult.Success)
            {
                var k = account;
                TriggerEvent(x => x.LoggedIn(k.Id, k.AccountName, k.Email));
                Thread.CurrentPrincipal = PrincipalFactory.CreatePrincipal(account);
            }


            return true;
        }

        /// <summary>
        /// Log out current user
        /// </summary>
        /// <remarks>
        /// Will set the <c>Thread.CurrentPrincipal</c> to not being authenticated
        /// </remarks>
        public void Logout()
        {
            var user = Thread.CurrentPrincipal.Identity.Name;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("") {}, new string[0]);
            TriggerEvent(x => x.LoggedOut(user));
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <exception cref="PasswordValidationException">Password do not follow the specified rules.</exception>
        /// <seealso cref="IPasswordValidator"/>
        public Account Register(string accountName, string email, string password)
        {
            if (_storage.LoadByAccountName(accountName) != null)
                throw new AccountNameInUseException(accountName);

            PasswordValidator.Validate(password);
            var context = new HashPasswordStrategyContext(password);
            PasswordStrategy.Encode(context);

            var account = CreateAccount(accountName, email, context.EncodedPassword);
            if (UseActivation)
            {
                account.GenerateActivationCode();
            }

            _storage.Store(account);
            TriggerEvent(x => x.Registered(account.Id, accountName, email));
            return account;
        }

        /// <summary>
        /// Tries the login using provider.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerId">The provider id.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        /// <exception cref="AccountLockedException"></exception>
        public bool TryLoginUsingProvider(string providerName, string providerId, string ipAddress, out Account account)
        {
            account = null;
            var token = _storage.GetTokenFromProvider(providerName, providerId);
            if (token == null || token.HasExpired)
                return false;

            account = _storage.LoadByAccountName(token.AccountName);
            if (account == null)
                return false;

            if (account.State == AccountState.Locked)
                throw new AccountLockedException(account.AccountName);

            var tmp = account;
            var result = account.Login();
            _storage.Store(account);
            TriggerEvent(x => x.LoggedIn(tmp.Id, tmp.AccountName, tmp.Email));
            Thread.CurrentPrincipal = PrincipalFactory.CreatePrincipal(account);
            return result == LoginResult.Success;
        }

        /// <summary>
        /// Gets if the specified username exists as a local account (i.e. got a password)
        /// </summary>
        /// <param name="accountName">username</param>
        /// <returns>
        ///   <c>true</c> if a local account has been created.
        /// </returns>
        public bool IsLocal(string accountName)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            var account = _storage.LoadByAccountName(accountName);
            return account.Password != null;
        }

        /// <summary>
        /// Existses the specified account name.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <returns></returns>
        public bool Exists(string accountName)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            return _storage.LoadByAccountName(accountName) != null;
        }


        private void TriggerEvent(Action<IAccountServiceEvents> action)
        {
            if (Subscribers == null)
                return;

            foreach (var subscriber in Subscribers)
            {
                action(subscriber);
            }
        }

        #endregion

        /// <summary>
        /// Creates a new account object.
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="email">Email address</param>
        /// <param name="encodedPassword">Encrypted/Hashed password</param>
        /// <returns>Account object</returns>
        protected virtual Account CreateAccount(string accountName, string email, string encodedPassword)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            if (email == null) throw new ArgumentNullException("email");
            if (encodedPassword == null) throw new ArgumentNullException("encodedPassword");
            return new Account(accountName, email, encodedPassword);
        }

    }
}