using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    ///     A token is an association to another system
    /// </summary>
    /// <remarks>It can for instance be information for OpenAuth (using facebook etc) or just a HTTP cookie.</remarks>
    public class Token
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Token" /> class.
        /// </summary>
        /// <param name="accountName">Name of the account that the token is for.</param>
        /// <param name="providerName">Name of the provider ("facebook" or "cookie" etc).</param>
        /// <param name="providerUserId">User id in the provider system.</param>
        public Token(string accountName, string providerName, string providerUserId)
        {
            if (accountName == null) throw new ArgumentNullException("accountName");
            if (providerName == null) throw new ArgumentNullException("providerName");
            if (providerUserId == null) throw new ArgumentNullException("providerUserId");

            Category = "Token";
            ProviderUserId = providerUserId;
            ProviderName = providerName;
            AccountName = accountName;
            Expires = DateTime.Now.AddMonths(2);
        }

        /// <summary>
        ///     Gets name of the provider (such as "cookie" or "facebook")
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        ///     Gets identity in the other system (for cookies it's the cookie token)
        /// </summary>
        public string ProviderUserId { get; private set; }

        /// <summary>
        ///     Gets or sets type of category (such as "token" or "openauth").
        /// </summary>
        /// <remarks>Used when managing authentication to list all tokens of a certain type</remarks>
        public string Category { get; set; }

        /// <summary>
        ///     Gets or sets remote IP which which this token is allowed for (null = disable IP check)
        /// </summary>
        public string RemoteIp { get; set; }

        /// <summary>
        ///     Gets or sets when this token expires.
        /// </summary>
        /// <remarks>The account service will request deletion for too old tokens.</remarks>
        public DateTime Expires { get; set; }

        /// <summary>
        ///     Gets account name that this token is associated with.
        /// </summary>
        public string AccountName { get; private set; }

        /// <summary>
        ///     Gets if the token has expired (i.e. login again)
        /// </summary>
        public bool HasExpired
        {
            get { return DateTime.Now > Expires; }
        }
    }
}