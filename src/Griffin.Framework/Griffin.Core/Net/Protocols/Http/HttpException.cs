using System;
using System.Net;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// A HTTP exception
    /// </summary>
    /// <remarks>HTTP exceptions will automatically generate a custom error page with the specified status code,
    /// opposed to all other exceptions which will generate a Internal Server Error.</remarks>
    public class HttpException : Exception
    {
        public HttpException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            HttpStatusCode = (int)statusCode;
        }

        public HttpException(int statusCode, string message)
            : base(message)
        {
            HttpStatusCode = statusCode;
        }

        /// <summary>
        /// Gets status code
        /// </summary>
        public int HttpStatusCode { get; private set; }
    }
}