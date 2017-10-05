using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Net.Authentication
{
    /// <summary>
    /// Failed to authenticate
    /// </summary>
    
    public class AuthenticationDeniedException : Exception
    {
      
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AuthenticationDeniedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public AuthenticationDeniedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
