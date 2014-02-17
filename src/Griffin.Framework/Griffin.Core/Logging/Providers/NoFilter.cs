using System;

namespace Griffin.Logging.Providers
{
    /// <summary>
    /// Disable filtering.
    /// </summary>
    public class NoFilter : ILoggerFilter
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
        /// Will always return true
        /// </summary>
        /// <param name="typeThatWantToLog">Type that wants to get a logger to be able to log entries that have been written by the dev.</param>
        /// <returns>Always <c>true</c>. <c>true</c>.</returns>
        public bool IsSatisfiedBy(Type typeThatWantToLog)
        {
            return true;
        }
    }
}
