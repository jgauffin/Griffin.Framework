using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Container
{
    public interface IContainer
    {
        TService Resolve<TService>();
        IEnumerable<TService> ResolveAll<TService>();
        IContainerScope CreateScope();
    }
}
