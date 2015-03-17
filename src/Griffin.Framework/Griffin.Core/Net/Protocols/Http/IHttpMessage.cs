using System.IO;
using System.Text;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     Represents a request or response.
    /// </summary>
    public interface IHttpMessage
    {
        /// <summary>
        ///     First line in a HTTP message divided into parts (array with three items).
        /// </summary>
        /// <remarks>
        ///     <para>The content of the line depends on if this is a HTTP request </para>
        /// </remarks>
        string StatusLine { get; }

        /// <summary>
        ///     Version of the HTTP protocol
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Typically <c>HTTP/1.1</c> but can also be the old version <c>HTTP/1.0</c> or the new draft <c>HTTP/2.0</c>
        ///         (aka SPDY)
        ///     </para>
        /// </remarks>
        string HttpVersion { get; set; }

        /// <summary>
        ///     All HTTP headers.
        /// </summary>
        /// <remarks>
        ///     <para>Missing headers will return <c>null</c> as value</para>
        /// </remarks>
        IHeaderCollection Headers { get; }

        /// <summary>
        ///     Body in the HTTP message
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The body has not been modified or parsed in any way. The actual stream is either a <c>MemoryStream</c> or
        ///         <c>FileStream</c> depending on the
        ///         size of the body.
        ///     </para>
        ///     <para>
        ///         The implementation of this interface should have the control over the specified stream. That is, the stream
        ///         will always be disposed by this library when the message has been processed.
        ///         Hence you have to make sure that we can take control over the stream that you've specified.
        ///     </para>
        /// </remarks>
        Stream Body { get; set; }

        /// <summary>
        ///     Length of the body in bytes.
        /// </summary>
        int ContentLength { get; set; }

        /// <summary>
        ///     Content type without any parameters.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If you need to get the boundary etc, then use <c>request.Headers["Content-Type"]</c>.
        ///     </para>
        /// </remarks>
        string ContentType { get; set; }

        /// <summary>
        ///     The encoding used in the document (if it's text of some sort)
        /// </summary>
        Encoding ContentCharset { get; set; }

        /// <summary>
        ///     Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        /// <exception cref="System.FormatException">
        ///     Header name may not contain colon, CR or LF.
        ///     or
        ///     Header value may not contain colon, CR or LF.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         If a client or a server receives repeated frame header entries, only the first header entry SHOULD be used as
        ///         the value of header entry. Subsequent values are only used to maintain a history of state changes of the header
        ///         and MAY be ignored. This implementation will IGNORE all subsequent headers
        ///     </para>
        /// </remarks>
        void AddHeader(string name, string value);
    }
}