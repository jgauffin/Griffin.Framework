namespace Griffin.Logging.Loggers.Filters
{
    /// <summary>
    ///     Filters on log level
    /// </summary>
    /// <example>
    ///     <code>
    /// var logger = new ConsoleLogger();
    /// logger.LogFilter = new LogLevelFilter { MinLevel = LogLevel.Info };
    /// 
    /// var provider = new LogProvider();
    /// provider.Add(logger);
    /// 
    /// LogManager.Provider = provider;
    /// </code>
    /// </example>
    public class LogLevelFilter : ILogEntryFilter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogLevelFilter" /> class.
        /// </summary>
        public LogLevelFilter()
        {
            MinLevel = LogLevel.Debug;
            MaxLevel = LogLevel.Error;
        }

        /// <summary>
        ///     Minimum level (inclusive)
        /// </summary>
        /// <value>
        ///     Default is <see cref="LogLevel.Debug" />.
        /// </value>
        public LogLevel MinLevel { get; set; }

        /// <summary>
        ///     Maximum level (inclusive)
        /// </summary>
        /// <value>
        ///     Default is <see cref="LogLevel.Error" />.
        /// </value>
        public LogLevel MaxLevel { get; set; }


        /// <summary>
        ///     Check if the logger may write this entry.
        /// </summary>
        /// <param name="logEntry">Entry that the user want to write</param>
        /// <returns>
        ///     <c>true</c> if the entry can be written; otherwise <c>false</c>.
        /// </returns>
        public bool IsSatisfiedBy(LogEntry logEntry)
        {
            return logEntry.LogLevel >= MinLevel && logEntry.LogLevel <= MaxLevel;
        }
    }
}