using System;

namespace Griffin.Container
{
    /// <summary>
    /// A dependency was missing when we tried to resolve a service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Thrown when the requested service can be found, but one of the dependencies that the implementation of the service has is missing.
    /// </para>
    /// </remarks>
    public class DependencyMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyMissingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public DependencyMissingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}