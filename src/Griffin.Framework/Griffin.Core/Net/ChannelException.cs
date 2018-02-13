using System;
using System.Runtime.Serialization;

namespace Griffin.Net
{
    /// <summary>
    /// Channel failed to work as expected.
    /// </summary>
    
    public class ChannelException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner.</param>
        public ChannelException(string errorMessage, Exception inner)
            : base(errorMessage, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ChannelException(string errorMessage)
            : base(errorMessage)
        {
        }
        
    }
}