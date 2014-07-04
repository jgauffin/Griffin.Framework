using System;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Detected an exception (in a app domain so we use <c>exception.ToString()</c> to make sure that we are not missing the assembly that the exception is defined in)
    /// </summary>
    public class UnhandledExceptionStringEventArgs : EventArgs
    {
        public UnhandledExceptionStringEventArgs(string exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }

        public string Exception { get; private set; }
    }
}