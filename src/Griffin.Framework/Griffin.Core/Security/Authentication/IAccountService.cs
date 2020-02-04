using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Provider returning user to be authenticated.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Try to authenticate
        /// </summary>
        /// <param name="secret">Depends on the authentication type. For "basic" it's the password, while for "digest" it's the HA1 hash.</param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <remarks>
        /// User name can basically be anything. For instance name entered by user when using
        /// basic or digest authentication, or SID when using Windows authentication.
        /// </remarks>
        /// <exception cref="HttpException">Typically with status code 403 if too many attempts have been made or if the user is not allowed.</exception>
        Task<ClaimsPrincipal> Authenticate(AuthenticationContext context);

        /// <summary>
        /// Load a previously authenticated user.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<ClaimsPrincipal> LoadAsync(Uri uri, string userName);
    }
}