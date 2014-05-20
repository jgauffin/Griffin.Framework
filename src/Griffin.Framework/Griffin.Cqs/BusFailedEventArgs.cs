using System;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Used by bus:es when they fail due to an internal error.
    /// </summary>
    public class BusFailedEventArgs : EventArgs
    {
        private readonly Exception _exception;

        public BusFailedEventArgs(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            _exception = exception;
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}