using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.ApplicationServices
{
    /// <summary>
    /// Guarded services can be stopped/started/restarted by this library
    /// </summary>
    public interface IGuardedService
    {
        /// <summary>
        /// Returns if the service is currently running
        /// </summary>
        bool IsRunning { get; }
    }
}
