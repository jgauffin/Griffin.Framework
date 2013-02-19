using System;
using System.Linq;
using System.Security.Principal;

namespace Griffin.Framework.Authentication.Principals
{
    /// <summary>
    /// Factory which uses <see cref="GenericPrincipal"/> and <see cref="GenericIdentity"/>
    /// </summary>
    public class GenericPrincipalFactory : IPrincipalFactory
    {
        private readonly IRoleStorage _roleStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPrincipalFactory" /> class.
        /// </summary>
        /// <param name="roleStorage">Used to invoke <see cref="IRoleStorage.GetRoles"/>.</param>
        public GenericPrincipalFactory(IRoleStorage roleStorage)
        {
            if (roleStorage == null) throw new ArgumentNullException("roleStorage");
            _roleStorage = roleStorage;
        }

        /// <summary>
        /// Create a new principal for the specified account.
        /// </summary>
        /// <param name="account">User that as been authenticated.</param>
        /// <returns>Principal to use</returns>
        public IPrincipal CreatePrincipal(Account account)
        {
            if (account == null) throw new ArgumentNullException("account");
            return new GenericPrincipal(new GenericIdentity(account.AccountName), _roleStorage.GetRoles(account.AccountName).ToArray());
        }
    }
}
