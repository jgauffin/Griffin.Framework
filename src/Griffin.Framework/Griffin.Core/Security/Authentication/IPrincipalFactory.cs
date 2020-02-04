using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Griffin.Security.Authentication
{
    /// <summary>
    /// Used to create a principal when an user have been successfully authenticated.
    /// </summary>
    public interface IPrincipalFactory
    {
        /// <summary>
        /// Create a principal
        /// </summary>
        /// <param name="account">Account loaded by <see cref="IAccountRepository"/>.</param>
        /// <returns>Principal</returns>
        /// <exception cref="InvalidOperationException">Failed to create principal using the specified account</exception>
        /// <exception cref="ArgumentNullException">account</exception>
        Task<ClaimsPrincipal> CreatePrincipalAsync(UserAccount account);
    }
}
