using System;

namespace Griffin.Cqs
{
    /// <summary>
    ///     Information about why a handler failed.
    /// </summary>
    public class HandlerFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFailure"/> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="exception">The exception.</param>
        /// <exception cref="System.ArgumentNullException">
        /// handler
        /// or
        /// exception
        /// </exception>
        public HandlerFailure(object handler, Exception exception)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (exception == null) throw new ArgumentNullException("exception");
            Handler = handler;
            Exception = exception;
        }

        /// <summary>
        ///     Handler that failed
        /// </summary>
        public object Handler { get; private set; }

        /// <summary>
        ///     Why the handler failed
        /// </summary>
        public Exception Exception { get; private set; }
    }
}