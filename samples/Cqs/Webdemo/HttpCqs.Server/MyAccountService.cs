using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http.Authentication;

namespace HttpCqs.Server
{
    internal class MyAccountService : IAccountService
    {
        public Task<ClaimsPrincipal> Authenticate(AuthenticationContext context)
        {
            throw new NotImplementedException();
        }

        public Task<ClaimsPrincipal> LoadAsync(Uri uri, string userName)
        {
            throw new NotImplementedException();
        }
    }
}