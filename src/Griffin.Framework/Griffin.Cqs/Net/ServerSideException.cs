using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Server side have thrown an exception
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to distinguish local exceptions (i.e. client failure) from server side failures.
    /// </para>
    /// </remarks>
    
    public class ServerSideException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSideException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ServerSideException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSideException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ServerSideException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSideException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ServerSideException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}