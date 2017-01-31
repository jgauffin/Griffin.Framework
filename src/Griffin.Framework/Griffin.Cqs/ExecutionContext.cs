using System.Collections.Generic;

namespace Griffin.Cqs
{
    /// <summary>
    ///     The execution context
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        ///     Provides context specific information
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }

        /// <summary>
        ///     Can be supplied by hooking into the callbacks on the bus implementations.
        /// </summary>
        public object UserState { get; set; }
    }
}