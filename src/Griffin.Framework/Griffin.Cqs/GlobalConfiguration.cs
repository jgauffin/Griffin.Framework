using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Cqs.Authorization;

namespace Griffin.Cqs
{
    /// <summary>
    /// Configuration for different aspects of this library
    /// </summary>
    /// <remarks>
    /// <para>
    /// Class is not thread safe, you are strongly encouraged to just do the configuration during app startup.
    /// </para>
    /// </remarks>
    public class GlobalConfiguration
    {
        /// <summary>
        /// Use to authorize CQS objects before their handlers are executed.
        /// </summary>
        public static IAuthorizationFilter AuthorizationFilter { get; set; }
    }
}
