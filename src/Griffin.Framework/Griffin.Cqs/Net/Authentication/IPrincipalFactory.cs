using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Used to create a principal when an user have been successfully authenticated.
    /// </summary>
    public interface IPrincipalFactory
    {
        /// <summary>
        /// Create a principal
        /// </summary>
        /// <param name="account">Account which have been authenticated</param>
        /// <returns>Principal</returns>
        /// <exception cref="InvalidOperationException">Failed to create principal using the specified account</exception>
        /// <exception cref="ArgumentNullException">account</exception>
        Task<IPrincipal> CreatePrincipalAsync(IUserAccount account);
    }
}
