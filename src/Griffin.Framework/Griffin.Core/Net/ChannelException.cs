using System;
using System.Runtime.Serialization;

namespace Griffin.Net
{
    /// <summary>
    /// Channel failed to work as expected.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ChannelException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

    }
}