using System;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.Serializers
{
    /// <summary>
    ///     A decoder failed to decode request/response body
    /// </summary>
    
    public class DecoderFailureException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DecoderFailureException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public DecoderFailureException(string errorMessage) : base(errorMessage)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DecoderFailureException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner.</param>
        public DecoderFailureException(string errorMessage, Exception inner) : base(errorMessage,inner)
        {
        }
    }
}