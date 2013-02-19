using System.IO;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Response adapter.
    /// </summary>
    /// <seealso cref="IRequestAdapter"/>
    public interface IResponseAdapter
    {
        /// <summary>
        /// Set a header
        /// </summary>
        /// <param name="name">Name as defined in the HTTP 1.1 specification. Case insensitive</param>
        /// <param name="value">Header value. <c>null</c> to remove header</param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Set response status
        /// </summary>
        /// <param name="code">Status code.</param>
        /// <param name="reason">Reason phrase</param>
        void SetStatus(ResponseStatusCode code, string reason);

        /// <summary>
        /// Set response body stream
        /// </summary>
        /// <param name="stream">Stream to set</param>
        void SetBody(Stream stream);
    }
}