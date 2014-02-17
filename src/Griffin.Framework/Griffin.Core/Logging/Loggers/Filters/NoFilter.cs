namespace Griffin.Logging.Loggers.Filters
{
    /// <summary>
    ///     Can be used to disable filtering
    /// </summary>
    public class NoFilter : ILogEntryFilter
    {
        /// <summary>
        ///     Instance to use
        /// </summary>
        public static readonly NoFilter Instance = new NoFilter();

        /// <summary>
        ///     Prevents a default instance of the <see cref="NoFilter" /> class from being created.
        /// </summary>
        private NoFilter()
        {
        }

        /// <summary>
        /// Will always return true;
        /// </summary>
        /// <param name="logEntry">Entry that the user want to write</param>
        /// <returns>
        ///   <c>true</c>, always.
        /// </returns>
        public bool IsSatisfiedBy(LogEntry logEntry)
        {
            return true;
        }
    }
}