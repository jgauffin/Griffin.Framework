using System;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    ///     Failed to handle STOMP request
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="errorMessage">The error message.</param>
        public BadRequestException(IFrame request, string errorMessage)
            : base(errorMessage)
        {
            Request = request;
        }

        /// <summary>
        ///     Frame that was not successfully handled.
        /// </summary>
        public IFrame Request { get; private set; }
    }

    
}