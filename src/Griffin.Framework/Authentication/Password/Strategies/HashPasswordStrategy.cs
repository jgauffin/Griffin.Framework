using System;

namespace Griffin.Framework.Authentication.Password.Strategies
{
    /// <summary>
    /// Uses <see cref="PasswordHash"/> class which stores the salt and the password in the same field (with colon as delimiter)
    /// </summary>
    public class HashPasswordStrategy : IPasswordStrategy
    {
        #region IPasswordStrategy Members

        /// <summary>
        /// Validate password during login
        /// </summary>
        /// <param name="context">Password information. The context depends on which strategy is used</param>
        /// <returns><c>true</c> if passwords match; otherwise false;</returns>
        public bool Compare(ComparePasswordContext context)
        {
            return PasswordHash.ValidatePassword(context.ClearTextPassword, context.StoredPassword);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Encode(IPasswordEncodeContext context)
        {
            context.EncodedPassword = PasswordHash.CreateHash(context.ClearTextPassword);
        }

        #endregion
    }

    /// <summary>
    /// Context for <see cref="HashPasswordStrategy.Encode"/>.
    /// </summary>
    public class HashPasswordStrategyContext : IPasswordEncodeContext
    {
        public HashPasswordStrategyContext(string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            ClearTextPassword = password;
        }

        /// <summary>
        /// Gets password as specified by the user
        /// </summary>
        public string ClearTextPassword { get; private set; }

        /// <summary>
        /// Gets or sets the encoded password
        /// </summary>
        public string EncodedPassword { get; set; }
    }
}