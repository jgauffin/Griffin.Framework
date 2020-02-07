using System.Threading.Tasks;
using Griffin.Security;
using Griffin.Security.Authentication;
using MicroMsg.Cqs.Messages;

namespace MicroMsg.Cqs.Services
{
    internal class FakeFetcher : IAccountRepository
    {
        PasswordHasherRfc2898 _hasher = new PasswordHasherRfc2898();

        public Task<UserAccount> FindByUserName(string userName)
        {
            var salt = _hasher.CreateSalt();
            return Task.FromResult<UserAccount>(new FakeUser()
            {
                HashedPassword = _hasher.HashPassword("mamma", salt),
                PasswordSalt = salt,
                IsLocked = false,
                UserName = "jonas"
            });
        }

        public Task IncreaseAttempts(UserAccount account)
        {
            ((FakeUser)account).Attempts++;
            return Task.CompletedTask;
        }
    }
}