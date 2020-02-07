using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http.Authentication;

namespace MicroMsg.Cqs.Services
{
    public class SimpleAccountService : IAccountService
    {
        public Task<ClaimsPrincipal> Authenticate(AuthenticationContext context)
        {
            return Task.FromResult(new ClaimsPrincipal());
        }

        public Task<ClaimsPrincipal> LoadAsync(Uri uri, string userName)
        {
            return Task.FromResult(new ClaimsPrincipal());
        }
    }
}