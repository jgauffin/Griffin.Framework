using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Security.Authentication
{
    public interface IAccountRepository
    {
        Task<UserAccount> FindByUserName(string userName);
        Task IncreaseAttempts(UserAccount account);

    }
}
