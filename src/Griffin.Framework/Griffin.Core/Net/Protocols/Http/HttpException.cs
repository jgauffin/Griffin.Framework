using System;
using System.Net;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     A HTTP exception
    /// </summary>
    /// <remarks>
    ///     HTTP exceptions will automatically generate a custom error page with the specified status code,
    ///     opposed to all other exceptions which will generate a Internal Server Error.
    /// </remarks>
    public class HttpException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="message">The message.</param>
        public HttpException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            HttpCode = (int) statusCode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public HttpException(int code, string message)
            : base(message)
        {
            HttpCode = code;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected HttpException(HttpStatusCode statusCode, string errorMessage, Exception inner)
            : base(errorMessage, inner)
        {
            HttpCode = (int) statusCode;
        }


        /// <summary>
        ///     Gets status code
        /// </summary>
        public int HttpCode { get; private set; }
    }
}