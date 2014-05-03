using System;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Response cookies have to specify where and when they are valid.
    /// </summary>
    public interface IResponseCookie : IHttpCookie
    {
        /// <summary>
        /// Gets domain that the cookie is valid under
        /// </summary>
        /// <remarks><c>null</c> means not specified</remarks>
        string Domain { get; set; }

        /// <summary>
        /// Gets when the cookie expires.
        /// </summary>
        /// <remarks><see cref="DateTime.MinValue"/> means that the cookie expires when the session do so.</remarks>
        DateTime ExpiresUtc { get; set; }

        /// <summary>
        /// Gets path that the cookie is valid under.
        /// </summary>
        /// <remarks><c>null</c> means not specified</remarks>
        string Path { get; set; }
    }
}