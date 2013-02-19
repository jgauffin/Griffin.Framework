namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Status for login using <see cref="IAccountService"/>.
    /// </summary>
    public enum LoginResult
    {
        /// <summary>
        /// Successful account
        /// </summary>
        Success,

        /// <summary>
        /// Incorrect Username Or Password
        /// </summary>
        IncorrectUsernameOrPassword,

        /// <summary>
        /// Acount have been locked (user have missbehaved)
        /// </summary>
        Locked
    }
}