namespace Griffin.Logging
{
    /// <summary>
    ///     Decides which log entries each logger should accept.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The logging library can use to types of filters. Logger filters which decide which log each type should write
    ///         to. The other kind of filters are implemented by this class: Once a logger receives a log entry, these filters
    ///         are used to determine which log entry can be logged.
    ///     </para>
    /// </remarks>
    public interface ILogEntryFilter
    {
        /// <summary>
        ///     Check if the logger may write this entry.
        /// </summary>
        /// <param name="logEntry">Entry that the user want to write</param>
        /// <returns><c>true</c> if the entry can be written; otherwise <c>false</c>.</returns>
        bool IsSatisfiedBy(LogEntry logEntry);
    }
}