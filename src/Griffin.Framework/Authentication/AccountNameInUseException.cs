using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// thrown by <see cref="AccountService"/> if a user tries to register with an occupied account name.
    /// </summary>
    public class AccountNameInUseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountNameInUseException" /> class.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        public AccountNameInUseException(string accountName)
            : base("Account name is already taken: " + accountName)
        {
            AccountName = accountName;
        }

        /// <summary>
        /// Gets tried account name
        /// </summary>
        public string AccountName { get; private set; }
    }
}