using System;
using System.Security.Principal;
using Griffin.Framework.Authentication.Password;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Account used during authentication and authorization.
    /// </summary>
    /// <remarks>
    /// <para>It's recommended that the password have been encrypted or hashed before being stored in the
    /// data storage. There are several built in techniques (see all implementations of <see cref="IPasswordStrategy"/>) that
    /// you can use to handle the password. Do note that some authentication techniques requires that the password
    /// is stored in a certain form. For instance Digest Authentication either needs a clear text password or a HA1 hash.</para>
    /// </remarks>
    public class Account
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="email"></param>
        /// <param name="password">The users password. Hopfully hashed or encrypted.</param>
        /// <remarks>See the class documentation for more information about the password handling.</remarks>
        public Account(string accountName, string email, string password)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            if (email == null) throw new ArgumentNullException("email");
            if (password == null) throw new ArgumentNullException("password");

            AccountName = accountName;
            Email = email;
            Password = password;
            CreatedAt = DateTime.Now;
            LastLoginAt = DateTime.Now;
            LoginAttempts = 0;
            State = AccountState.NotActivated;
            PasswordOption = "";
        }

        /// <summary>
        /// Gets account name. Will also be the one used in the <see cref="IIdentity"/> when the user is logged in.
        /// </summary>
        public string AccountName { get; private set; }

        /// <summary>
        /// Gets email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets when the user logged in last.
        /// </summary>
        public DateTime LastLoginAt { get; private set; }

        /// <summary>
        /// Gets the number of unsuccessful login attempts.
        /// </summary>
        /// <remarks>Will be reseted on a successful login</remarks>
        public int LoginAttempts { get; private set; }

        /// <summary>
        /// Gets current state of the account
        /// </summary>
        public AccountState State { get; private set; }

        /// <summary>
        /// Gets or sets code that the user must use to activate the account
        /// </summary>
        public string ActivationCode { get; private set; }

        /// <summary>
        /// Used to generate an activation code which is required to be able to login
        /// </summary>
        /// <returns>Generated code</returns>
        public string GenerateActivationCode()
        {
            ActivationCode = Guid.NewGuid().ToString("N");
            State = AccountState.NotActivated;
            return ActivationCode;
        }

        /// <summary>
        /// Activate account using a code entered by the suer
        /// </summary>
        /// <param name="suppliedCode">Code typed by user.</param>
        /// <returns><c>true</c> if the code matches the supplied one; otherwise <c>false</c>.</returns>
        /// <remarks>Will delete the code activation is not successufl.</remarks>
        public bool Activate(string suppliedCode)
        {
            if (suppliedCode == null) throw new ArgumentNullException("suppliedCode");
            if (suppliedCode != ActivationCode)
                return false;

            State = AccountState.Active;
            return true;
        }

        /// <summary>
        /// Gets password as stored in the data source
        /// </summary>
        /// <remarks>The password is probably encrypted or hashed depending on the used <see cref="IPasswordStrategy"/>.</remarks>
        public string Password { get; private set; }

        /// <summary>
        /// Gets extra password parameter.
        /// </summary>
        /// <remarks>The contents of this field depends on the password strategy. <para>For instance Digest authentication should store the "realm" here, while hashed passwords should store the salt.</para></remarks>
        public string PasswordOption { get; private set; }

        /// <summary>
        /// ProviderUserId in the data source.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets when account was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Increase the number of login attempts
        /// </summary>
        /// <param name="maxAttempts">Maximum number of allowed attempts</param>
        /// <returns>true if we may still try to login; otherwise <c>false</c>.</returns>
        private LoginResult IncreaseLoginAttempts(int maxAttempts)
        {
            LoginAttempts++;
            if (LoginAttempts >= maxAttempts)
            {
                State = AccountState.Locked;
                return LoginResult.Locked;
            }

            return LoginResult.IncorrectUsernameOrPassword;
        }

        public LoginResult Login()
        {
            if (State != AccountState.Active)
                return LoginResult.Locked;

            LoginAttempts = 0;
            State = AccountState.Active;
            LastLoginAt = DateTime.Now;
            return LoginResult.Success;
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            return true;
        }

        /// <summary>
        /// Tries the login.
        /// </summary>
        /// <param name="strategy">Password strategy to use for authentication.</param>
        /// <param name="maxAttempts">Max number of attempts.</param>
        /// <param name="password">Password, as entered by the user..</param>
        /// <returns></returns>
        public LoginResult TryLogin(IPasswordStrategy strategy, int maxAttempts, string password)
        {
            if (strategy.Compare(new ComparePasswordContext(Password, password, PasswordOption)))
            {
                return Login();
            }

            return IncreaseLoginAttempts(maxAttempts);
        }
    }

}