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