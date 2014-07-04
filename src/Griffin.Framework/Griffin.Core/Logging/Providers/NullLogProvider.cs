using System;
using Griffin.Logging.Loggers;

namespace Griffin.Logging.Providers
{
    /// <summary>
    /// Just returns <see cref="NullLogger.Instance"/> for every request.
    /// </summary>
    public class NullLogProvider : ILogProvider
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type that wants to log.</param>
        /// <returns></returns>
        public ILogger GetLogger(Type type)
        {
            return NullLogger.Instance;
        }
    }
}
