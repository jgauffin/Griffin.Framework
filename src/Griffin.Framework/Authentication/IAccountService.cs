using Griffin.Framework.Authentication.Principals;

namespace Griffin.Framework.Authentication
{
    public interface IAccountService
    {
        /// <summary>
        /// Gets or sets if all new accounts have to be activated using activation codes sent by email.
        /// </summary>
        /// <remarks>You are yourself responsible of sending out the email. The created account will have an activation code after that the <see cref="Register"/> method has been invoked.</remarks>
        bool UseActivation { get; set; }

        /// <summary>
        /// Log out current user
        /// </summary>
        /// <remarks>Will set the <c>Thread.CurrentPrincipal</c> to not being authenticated</remarks>
        void Logout();

        /// <summary>
        /// Register a new local account
        /// </summary>
        /// <param name="accountName">Account to register with</param>
        /// <param name="email">Email address (used to send activation email if required)</param>
        /// <param name="password">Password used to login</param>
        /// <exception cref="AccountNameInUseException">Someone has already registered using the specified account name.</exception>
        Account Register(string accountName, string email, string password);

        /// <summary>
        /// Will login without any checks.
        /// </summary>
        /// <param name="accountName">Account to log into.</param>
        /// <param name="account">Account which was logged in</param>
        /// <returns>Login result</returns>
        /// <remarks>Will assign the <c>Thread.CurrentPrincipal</c> with the corrent principal if login was successful (using the <see cref="IPrincipalFactory"/>).</remarks>
        /// <seealso cref="IPrincipalFactory"/>
        /// <exception cref="AccountLockedException">Accound is locked</exception>
        bool TryLogin(string accountName, out Account account);

        /// <summary>
        /// Standard login
        /// </summary>
        /// <param name="accountName">Account to log into.</param>
        /// <param name="password">Password</param>
        /// <param name="account">Account which was logged in</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c></returns>
        /// <remarks>Will assign the <c>Thread.CurrentPrincipal</c> with the corrent principal if login was successful (using the <see cref="IPrincipalFactory"/>).</remarks>
        /// <seealso cref="IPrincipalFactory"/>
        /// <exception cref="AccountLockedException">Accound is locked</exception>
        bool TryLogin(string accountName, string password, out Account account);

        /// <summary>
        /// Authenticate user using special credentials.
        /// </summary>
        /// <param name="providerName">"cookie", "apikey", "facebook" etc.</param>
        /// <param name="providerId">Authentication providerId (which was previously generated during a manual login)</param>
        /// <param name="remoteIp">IP address that the user login from</param>
        /// <param name="account">Account for the logged in user.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        /// <remarks>Will assign the <c>Thread.CurrentPrincipal</c> with the corrent principal if login was successful (using the <see cref="IPrincipalFactory"/>).</remarks>
        /// <seealso cref="IPrincipalFactory"/>
        /// <exception cref="AccountLockedException">Accound is locked</exception>
        bool TryLoginUsingProvider(string providerName, string providerId, string remoteIp, out Account account);

        /// <summary>
        /// Gets if the specified username exists as a local account (i.e. got a password)
        /// </summary>
        /// <param name="accountName">username</param>
        /// <returns><c>true</c> if a local account has been created.</returns>
        bool IsLocal(string accountName);

        /// <summary>
        /// Checks if the account exists.
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <returns></returns>
        bool Exists(string accountName);
    }
}