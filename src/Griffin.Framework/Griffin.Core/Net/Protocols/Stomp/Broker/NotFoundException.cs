using System;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    /// <summary>
    /// Something was not found, like a queue.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public NotFoundException(string errorMessage)
            : base(errorMessage)
        {
            
        }
    }
}