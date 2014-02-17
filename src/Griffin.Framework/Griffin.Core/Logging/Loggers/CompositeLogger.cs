using System;
using System.Collections.Generic;

namespace Griffin.Logging.Loggers
{
    /// <summary>
    ///     Can be used to write to several logs in one run.
    /// </summary>
    public class CompositeLogger : BaseLogger
    {
        private readonly IList<ILogger> _loggers = new List<ILogger>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeLogger" /> class.
        /// </summary>
        /// <param name="typeThatLogs">
        ///     Type of the class which uses this log. The type is used to write in the log file where the
        ///     lines come from.
        /// </param>
        public CompositeLogger(Type typeThatLogs) : base(typeThatLogs)
        {
        }

        /// <summary>
        ///     Add another logger that each entry should be written to
        /// </summary>
        /// <param name="logger">Logger to write to</param>
        public void Add(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _loggers.Add(logger);
        }

        /// <summary>
        ///     Write entry to the destination.
        /// </summary>
        /// <param name="entry">Entry to write</param>
        /// <exception cref="System.ArgumentNullException">entry</exception>
        public override void Write(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            foreach (var logger in _loggers)
            {
                logger.Write(entry);
            }
        }
    }
}