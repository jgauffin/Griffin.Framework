using System.Collections.Generic;
using System.Security.Principal;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Used to be able to generate <see cref="IPrincipal" /> directly.
    /// </summary>
    public interface IUserWithRoles : IAccountService
    {
        /// <summary>
        /// Get a list of all roles
        /// </summary>
        IEnumerable<string> RoleNames { get; }
    }
}