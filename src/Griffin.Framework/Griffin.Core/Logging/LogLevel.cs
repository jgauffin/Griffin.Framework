using Griffin.Logging.Loggers;

namespace Griffin.Logging
{
    /// <summary>
    /// Log levels
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Very detailed logs used during diagnostics
        /// </summary>
        /// <remarks>
        /// Should not be used other when trying to find hard-to-locate errors. You can disable trace output by using a <see cref="ILogEntryFilter"/>.
        /// </remarks>
        Trace,

        /// <summary>
        /// Diagnostics (like executing a SQL query)
        /// </summary>
        Debug,

        /// <summary>
        /// Events and similar (for instance that a user have logged in)
        /// </summary>
        Info,

        /// <summary>
        /// Something failed, but processing can continue. 
        /// </summary>
        Warning,

        /// <summary>
        /// Something failed, expected execution path can not succeed.
        /// </summary>
        Error
    }
}