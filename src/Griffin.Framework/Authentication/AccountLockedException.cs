using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Account has been locked, login is therefore not possible
    /// </summary>
    public class AccountLockedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountLockedException" /> class.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        public AccountLockedException(string accountName)
            : base("Account is locked: " + accountName)
        {
            
        }
    }
}
