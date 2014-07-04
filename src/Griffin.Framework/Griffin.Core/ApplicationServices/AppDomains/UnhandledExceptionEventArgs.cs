using System;
using Griffin.ApplicationServices.AppDomains.Controller;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Detected an exception.
    /// </summary>
    public class UnhandledExceptionEventArgs : EventArgs
    {
        public UnhandledExceptionEventArgs(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}