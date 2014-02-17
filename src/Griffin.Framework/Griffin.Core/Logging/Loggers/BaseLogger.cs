using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Griffin.Logging.Loggers
{
    /// <summary>
    /// Base class for loggers.
    /// </summary>
    /// <remarks>All you have to do is to override <see cref="Write"/>.</remarks>
    public abstract class BaseLogger : ILogger
    {
        private readonly Type _loggedType;
        private ILogEntryFilter _logFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLogger"/> class.
        /// </summary>
        /// <param name="typeThatLogs">Type of the class which uses this log. The type is used to write in the log file where the lines come from.</param>
        protected BaseLogger(Type typeThatLogs)
        {
            if (typeThatLogs == null) throw new ArgumentNullException("typeThatLogs");
            _loggedType = typeThatLogs;
        }

       
        /// <summary>
        /// Gets the type for the class which logs using this class
        /// </summary>
        public Type LoggedType
        {
            get { return _loggedType; }
        }

        public ILogEntryFilter LogFilter
        {
            get { return _logFilter; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _logFilter = value;
            }
        }

        #region ILogger Members

        /// <summary>
        /// Detailed framework messages used to find wierd errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Trace(string message, params object[] formatters)
        {
            Write(LogLevel.Trace, string.Format(message, formatters), null);
        }

        /// <summary>
        /// Detailed framework messages used to find wierd errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        public void Trace(string message, Exception exception)
        {
            Write(LogLevel.Trace, message, exception);
        }

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Debug(string message, params object[] formatters)
        {
            Write(LogLevel.Debug, string.Format(message, formatters), null);
        }

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Debug(string message, Exception exception)
        {
            Write(LogLevel.Debug, message, exception);
        }

        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Info(string message, params object[] formatters)
        {
            Write(LogLevel.Info, string.Format(message, formatters), null);
        }

        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        public void Info(string message, Exception exception)
        {
            Write(LogLevel.Info, message, exception);
        }

        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Warning(string message, params object[] formatters)
        {
            Write(LogLevel.Warning, string.Format(message, formatters), null);
        }

        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Warning(string message, Exception exception)
        {
            Write(LogLevel.Warning, message, exception);
        }

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        public void Error(string message, params object[] formatters)
        {
            Write(LogLevel.Error, string.Format(message, formatters), null);
        }

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        public void Error(string message, Exception exception)
        {
            Write(LogLevel.Error, message, exception);
        }

        #endregion

        /// <summary>
        /// Checks our log filter and then calls the abstract method
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="msg">Message to write.</param>
        /// <param name="exception">The exception (or null).</param>
        protected virtual void Write(LogLevel logLevel, string msg, Exception exception)
        {
            var entry = new LogEntry(logLevel, msg, exception);
            if (!LogFilter.IsSatisfiedBy(entry))
                return;

            Write(entry);
        }

        /// <summary>
        /// Write entry to the destination.
        /// </summary>
        /// <param name="entry">Entry to write</param>
        public abstract void Write(LogEntry entry);

        /// <summary>
        /// Formats exception details (including all inner exceptions)
        /// </summary>
        /// <param name="exception">Thrown exception.</param>
        /// <param name="spaces">Number of spaces to prefix each line with.</param>
        /// <param name="result">The created information will be appended to this string builder.</param>
        /// <remarks>Increases the number of spaces for each inner exception so it's easy to see all information</remarks>
        protected virtual void BuildExceptionDetails(Exception exception, int spaces, StringBuilder result)
        {
            result.Append("".PadLeft(spaces));
            result.AppendLine(exception.ToString().Replace("\r\n", "\r\n" + spaces));
            var properties = exception
                .GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                .ToList();
            if (properties.Count > 0)
            {
                result.Append("".PadLeft(spaces));
                result.Append("[");
                foreach (var propertyInfo in properties)
                {
                    var value = propertyInfo.GetValue(exception);
                    if (value == null)
                        continue;

                    result.Append(propertyInfo.Name);
                    result.Append("='");
                    result.Append(value);
                    result.Append("',");
                }
                result.Remove(result.Length - 1, 1);
                result.AppendLine("]");
            }

            if (exception.InnerException != null)
                BuildExceptionDetails(exception.InnerException, spaces + 4, result);
        }
    }
}