using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    public interface IPrincipalFactory
    {
        Task<IPrincipal> CreatePrincipalAsync(IUserAccount account);
    }
}
