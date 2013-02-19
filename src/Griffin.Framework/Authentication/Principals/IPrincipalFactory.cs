using System.Security.Principal;

namespace Griffin.Framework.Authentication.Principals
{
    /// <summary>
    /// Created to allow you to use your own custom principal.
    /// </summary>
    public interface IPrincipalFactory
    {
        /// <summary>
        /// Create a new principal for the specified account.
        /// </summary>
        /// <param name="account">User that as been authenticated.</param>
        /// <returns>Principal to use</returns>
        IPrincipal CreatePrincipal(Account account);
    }
}