using System;
using Griffin.ApplicationServices.AppDomains.Controller;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Detected an exception.
    /// </summary>
    public class UnhandledExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <exception cref="System.ArgumentNullException">exception</exception>
        public UnhandledExceptionEventArgs(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }

        /// <summary>
        /// Exception that occurred.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}