using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    /// <summary>
    ///     Creates a <c>GenericPrincipal</c> using a <c>IUserFetcher</c>.
    /// </summary>
    public class GenericPrincipalFactory : IPrincipalFactory
    {
        private readonly IUserFetcher _fetcher;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericPrincipalFactory" /> class.
        /// </summary>
        /// <param name="fetcher">Used to load roles for authenticated users.</param>
        /// <exception cref="System.ArgumentNullException">fetcher</exception>
        public GenericPrincipalFactory(IUserFetcher fetcher)
        {
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            _fetcher = fetcher;
        }


        /// <summary>
        ///     Create a principal
        /// </summary>
        /// <param name="account">Account which have been authenticated</param>
        /// <returns>Principal</returns>
        /// <exception cref="InvalidOperationException">Failed to create principal using the specified account</exception>
        /// <exception cref="ArgumentNullException">account</exception>
        public async Task<IPrincipal> CreatePrincipalAsync(IUserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            var roles = await _fetcher.GetRolesAsync(account);
            return new GenericPrincipal(new GenericIdentity(account.UserName, "ThreeWay"), roles);
        }
    }
}