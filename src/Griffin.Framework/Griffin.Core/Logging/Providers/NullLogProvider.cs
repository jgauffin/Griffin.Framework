using System;
using Griffin.Logging.Loggers;

namespace Griffin.Logging.Providers
{
    /// <summary>
    /// Just returns <see cref="NullLogger.Instance"/> for every request.
    /// </summary>
    public class NullLogProvider : ILogProvider
    {
        public ILogger GetLogger(Type type)
        {
            return NullLogger.Instance;
        }
    }
}
