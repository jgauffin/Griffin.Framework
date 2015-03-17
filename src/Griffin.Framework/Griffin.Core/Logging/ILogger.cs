using System;

namespace Griffin.Logging
{
    /// <summary>
    /// Logging interface
    /// </summary>
    /// <remarks>You typically just want to log the warnings and the errors from the framework since your logs
    /// will probably be filled very quickly otherwise.</remarks>
    public interface ILogger
    {
        /// <summary>
        /// Detailed framework messages used to find weird errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        void Trace(string message, params object[] formatters);


        /// <summary>
        /// Detailed framework messages used to find weird errors.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        void Trace(string message, Exception exception);

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        void Debug(string message, params object[] formatters);

        /// <summary>
        /// Diagnostic messages. Not as detailed as the trace messages but still only useful during debugging.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        void Debug(string message, Exception exception);

        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        void Info(string message, params object[] formatters);


        /// <summary>
        /// Information messages are typically used when the application changes state somewhere.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Thrown exception</param>
        void Info(string message, Exception exception);


        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        void Warning(string message, params object[] formatters);


        /// <summary>
        /// Something did not go as planned, but the framework can still continue as expected.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        void Warning(string message, Exception exception);

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="formatters">Formatters used in the <c>message</c>.</param>
        void Error(string message, params object[] formatters);

        /// <summary>
        /// Something failed. The framework must abort the current processing
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception which has been thrown</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// Write a previously created log instance.
        /// </summary>
        /// <param name="entry">Entry to write to the logfile.</param>
        void Write(LogEntry entry);
    }
}