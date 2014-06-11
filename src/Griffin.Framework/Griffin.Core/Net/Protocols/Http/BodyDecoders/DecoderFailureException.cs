using System;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.Http.BodyDecoders
{
    /// <summary>
    ///     A decoder failed to decode request/response body
    /// </summary>
    [Serializable]
    public class DecoderFailureException : BadRequestException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DecoderFailureException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public DecoderFailureException(string errorMessage) : base(errorMessage)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DecoderFailureException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected DecoderFailureException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
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