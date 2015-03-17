using System;

namespace Griffin.Logging.Loggers
{
    /// <summary>
    /// Throws away all logs
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        #region ILogger Members

        

        #endregion

        /// <summary>
        /// Detailed framework messages used to find weird errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Trace(string message, params object[] formatters)
        {
            
        }

        /// <summary>
        /// Detailed framework messages used to find weird errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        public void Trace(string message, Exception exception)
        {
            
        }

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Debug(string message, params object[] formatters)
        {
            
        }

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Debug(string message, Exception exception)
        {
            
        }

        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Info(string message, params object[] formatters)
        {
            
        }

        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        public void Info(string message, Exception exception)
        {
        }

        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Warning(string message, params object[] formatters)
        {
            
        }

        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Warning(string message, Exception exception)
        {
            
        }

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Error(string message, params object[] formatters)
        {
            
        }

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Error(string message, Exception exception)
        {
            
        }

        /// <summary>
        /// Write a previously created log instance.
        /// </summary>
        /// <param name="entry">Entry to write to the logfile.</param>
        public void Write(LogEntry entry)
        {
        }
    }
}