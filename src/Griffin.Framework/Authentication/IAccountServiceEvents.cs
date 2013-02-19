namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Used to subcribe on events for the <see cref="AccountService"/>.
    /// </summary>
    public interface IAccountServiceEvents
    {
        /// <summary>
        /// A user have logged in.
        /// </summary>
        /// <param name="id">ProviderUserId in the account data storage</param>
        /// <param name="accountName">Account name</param>
        /// <param name="email">Email.</param>
        void LoggedIn(string id, string accountName, string email);

        /// <summary>
        /// A new user have registered an account
        /// </summary>
        /// <param name="id">ProviderUserId in the account data storage</param>
        /// <param name="accountName">Account name</param>
        /// <param name="email">Email.</param>
        void Registered(string id, string accountName, string email);

        /// <summary>
        /// A user have logged out (clicked on log out)
        /// </summary>
        /// <param name="accountName">User account name</param>
        void LoggedOut(string accountName);
    }
}