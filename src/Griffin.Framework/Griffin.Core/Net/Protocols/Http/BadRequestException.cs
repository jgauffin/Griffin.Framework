using System;
using System.Net;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     The request was malformed.
    /// </summary>
    /// <remarks>
    ///     <para>Uses 400 as status code</para>
    /// </remarks>
    [Serializable]
    public class BadRequestException : HttpException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public BadRequestException(string errorMessage)
            : base(HttpStatusCode.BadRequest, errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">Inner exception.</param>
        public BadRequestException(string errorMessage, Exception inner)
            : base(HttpStatusCode.BadRequest, errorMessage, inner)
        {
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected BadRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}