using System;

namespace Griffin.Logging
{
    /// <summary>
    ///     Used to determine which loggers a specific type can log to.
    /// </summary>
    public interface ILoggerFilter
    {
        /// <summary>
        ///     Checks if the specified logger may log to a certain logger.
        /// </summary>
        /// <param name="typeThatWantToLog">Type that want's to write to a log.</param>
        /// <returns><c>true</c> if the logging type is acceptable by this filter; otherwise <c>false</c>.</returns>
        bool IsSatisfiedBy(Type typeThatWantToLog);
    }
}