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
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected HttpException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            HttpCode = info.GetInt32("HttpCode");
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

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
        ///     information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("HttpCode", HttpCode);
            base.GetObjectData(info, context);
        }
    }
}