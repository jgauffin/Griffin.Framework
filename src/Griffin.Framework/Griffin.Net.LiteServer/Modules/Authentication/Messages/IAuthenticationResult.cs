namespace Griffin.Net.LiteServer.Modules.Authentication.Messages
{
    /// <summary>
    /// Finaly step of the authentication process.
    /// </summary>
    public interface IAuthenticationResult
    {
        /// <summary>
        /// Returns wether the user may login or not
        /// </summary>
        AuthenticationResultState State { get; }
    }
}
