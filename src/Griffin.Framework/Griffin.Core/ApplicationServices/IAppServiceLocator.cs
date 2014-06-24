using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.ApplicationServices
{
    /// <summary>
    /// Used to discover services.
    /// </summary>
    public interface IAppServiceLocator
    {
        /// <summary>
        /// Discover all services for <see cref="ApplicationServiceManager"/>.
        /// </summary>
        /// <returns>Returned services are considered to be single instances, i.e. live as long as the application.</returns>
        IEnumerable<IApplicationService> GetServices();


    }
}
