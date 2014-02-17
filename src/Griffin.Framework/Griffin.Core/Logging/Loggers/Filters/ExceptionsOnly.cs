using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Logging.Loggers.Filters
{
    /// <summary>
    /// Only allow entries that got exceptions
    /// </summary>
    public class ExceptionsOnly : ILogEntryFilter
    {
        /// <summary>
        /// Check if the entry have an exception
        /// </summary>
        /// <param name="logEntry">Entry that the user want to write</param>
        /// <returns>
        ///   <c>true</c> if the entry has an exception; otherwise <c>false</c>.
        /// </returns>
        public bool IsSatisfiedBy(LogEntry logEntry)
        {
            return logEntry.Exception != null;
        }
    }
}
