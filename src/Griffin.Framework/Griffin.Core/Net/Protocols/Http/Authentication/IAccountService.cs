using System;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Provider returning user to be authenticated.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Lookups the specified user
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="host">Typically web server domain name.</param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <remarks>
        /// User name can basically be anything. For instance name entered by user when using
        /// basic or digest authentication, or SID when using Windows authentication.
        /// </remarks>
        /// <exception cref="HttpException">Typically with status code 403 if too many attempts have been made or if the user is not allowed.</exception>
        IAuthenticationUser Lookup(string userName, Uri host);

        /// <summary>
        /// Hash password to be able to do comparison
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        string HashPassword(string userName, Uri host, string password);

        /// <summary>
        /// Allows you to manage passwords by your self. Only works if the client supplies clear text passwords.
        /// </summary>
        /// <param name="user">User that your service returned from <c>Lookup</c>.</param>
        /// <param name="password">Password supplied by the client (web browser / http client)</param>
        /// <returns><c>true</c> if the user was authenticated</returns>
        bool ComparePassword(IAuthenticationUser user, string password);
    }
}