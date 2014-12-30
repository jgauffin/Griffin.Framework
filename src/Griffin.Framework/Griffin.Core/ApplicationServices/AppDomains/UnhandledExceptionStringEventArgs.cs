using System;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Detected an exception (in a app domain so we use <c>exception.ToString()</c> to make sure that we are not missing the assembly that the exception is defined in)
    /// </summary>
    public class UnhandledExceptionStringEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionStringEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <exception cref="System.ArgumentNullException">exception</exception>
        public UnhandledExceptionStringEventArgs(string exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }

        /// <summary>
        /// Exception that occurred.
        /// </summary>
        public string Exception { get; private set; }
    }
}