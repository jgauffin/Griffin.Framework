using System;
using System.Runtime.Serialization;

namespace Griffin.Net.Authentication
{
    /// <summary>
    ///     Thrown when the client needs to authenticate
    /// </summary>
    
    public class AuthenticationRequiredException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthenticationRequiredException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AuthenticationRequiredException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthenticationRequiredException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public AuthenticationRequiredException(string message, Exception inner) : base(message, inner)
        {
        }
        
    }
}