using System;

namespace Griffin.Logging
{
    /// <summary>
    /// Entry to write
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry" /> class.
        /// </summary>
        /// <param name="logLevel">The log level, see the enum for a detail description of each level..</param>
        /// <param name="message">Message written by the dev.</param>
        /// <param name="exception">exception (if any).</param>
        /// <exception cref="System.ArgumentNullException">message</exception>
        public LogEntry(LogLevel logLevel, string message, Exception exception)
        {
            if (message == null) throw new ArgumentNullException("message");

            LogLevel = logLevel;
            Message = message;
            Exception = exception;
            WrittenAt = DateTime.Now;
        }

        /// <summary>
        /// When the log entry was written
        /// </summary>
        public DateTime WrittenAt { get; set; }

        /// <summary>
        /// log level, see the enum for a detail description of each level
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Message written by the dev
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Exception if any.
        /// </summary>
        public Exception Exception { get; set; }
    }


}
