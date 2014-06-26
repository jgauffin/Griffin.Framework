namespace Griffin.Net.LiteServer.Modules.Authentication.Messages
{
    /// <summary>
    /// State for <see cref="IAuthenticationResult"/>
    /// </summary>
    public enum AuthenticationResultState
    {
        /// <summary>
        /// Successful login
        /// </summary>
        Success,

        /// <summary>
        /// Password failed.
        /// </summary>
        IncorrectLogin,

        /// <summary>
        /// Account have been locked (to many attempts or disabled account)
        /// </summary>
        Locked,

        /// <summary>
        /// Failed to login due to an error in the server application.
        /// </summary>
        ServerError
    }
}