using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Used to persist accounts
    /// </summary>
    public interface IAccountStorage
    {
        /// <summary>
        /// Load account using one of it's login tokens.
        /// </summary>
        /// <param name="providerName">Kind of source for the providerId, like "cookie" or "apikey".</param>
        /// <param name="providerUserId">Token to load by</param>
        /// <remarks>User name if found (and the providerId is still valid)</remarks>
        Token GetTokenFromProvider(string providerName, string providerUserId);

        /// <summary>
        /// Get a providerUserId
        /// </summary>
        /// <param name="accountName">account name</param>
        /// <param name="providerName">provider to get providerUserId for</param>
        /// <returns></returns>
        Token GetToken(string accountName, string providerName);

        /// <summary>
        /// Get all tokens for an user.
        /// </summary>
        /// <param name="accountName">Token to get.</param>
        /// <returns>Found tokens</returns>
        Token[] GetTokens(string accountName);

        /// <summary>
        /// Remove a providerUserId.
        /// </summary>
        /// <param name="accountName">Account name for the user.</param>
        /// <param name="providerName">Provider. for instance "facebook" or "cookie"</param>
        /// <exception cref="InvalidOperationException">Token is the last one and no local account exists.</exception>
        void RemoveToken(string accountName, string providerName);

        /// <summary>
        /// Load an account by it's account name
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <returns>Account if found; otherwise <c>null</c>.</returns>
        Account LoadByAccountName(string accountName);

        /// <summary>
        /// Store account in the data source.
        /// </summary>
        /// <param name="account">Account to store</param>
        /// <remarks>The account might be a new one or a previously created.</remarks>
        void Store(Account account);

        /// <summary>
        /// Store a new login providerId
        /// </summary>
        /// <param name="token">Token to store</param>
        void StoreToken(Token token);
    }
}