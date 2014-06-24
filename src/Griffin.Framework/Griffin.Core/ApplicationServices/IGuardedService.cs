using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.ApplicationServices
{
    /// <summary>
    /// Guarded services can be stopped/started/restarted by this library during runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use app/web.config to disable/enable the service (even during runtime).
    /// </para>
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///  <appSettings>
    ///    <add key="YourClassName.Enabled" value="true"/>
    ///  <appSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </remarks>
    public interface IGuardedService : IApplicationService
    {
        /// <summary>
        /// Returns if the service is currently running
        /// </summary>
        bool IsRunning { get; }


        /// <summary>
        /// Service failed to execute.
        /// </summary>
        event EventHandler<ApplicationServiceFailedEventArgs> Failed;
    }
}
