using System;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Used by bus:es when they fail due to an internal error.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The actual bus can be retrived from the <code>sender</code> argument in the event delegate.
    /// </para>
    /// </remarks>
    public class BusFailedEventArgs : EventArgs
    {
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusFailedEventArgs"/> class.
        /// </summary>
        /// <param name="exception">exception that the bus threw.</param>
        /// <exception cref="System.ArgumentNullException">exception</exception>
        public BusFailedEventArgs(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            _exception = exception;
        }

        /// <summary>
        /// Kind of failure that the bus experienced.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }
    }
}