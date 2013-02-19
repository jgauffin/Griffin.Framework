using System;

namespace Griffin.Framework.Exceptions
{
    /// <summary>
    /// Context 
    /// </summary>
    public class ExceptionFilterContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilterContext" /> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ExceptionFilterContext(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }

        /// <summary>
        /// Exception which was caught.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets information which might help during debugging.
        /// </summary>
        public virtual string ContextInformation { get { return ""; } }

        /// <summary>
        /// Gets or sets if the exception should be thrown
        /// </summary>
        /// <remarks>Overrides the <see cref="ExceptionFilters.ThrowExceptions"/> property</remarks>
        public bool ThrowException { get; set; }
    }
}