using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Creates a <c>GenericPrincipal</c> using a <c>IUserFetcher</c>.
    /// </summary>
    public class GenericPrincipalFactory : IPrincipalFactory
    {
        private readonly IUserFetcher _fetcher;

        public GenericPrincipalFactory(IUserFetcher fetcher)
        {
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            _fetcher = fetcher;
        }

       
        public async Task<IPrincipal> CreatePrincipalAsync(IUserAccount account)
        {
            var roles = await _fetcher.GetRolesAsync(account);
            return new GenericPrincipal(new GenericIdentity(account.UserName, "ThreeWay"), roles);
        }
    }
}
