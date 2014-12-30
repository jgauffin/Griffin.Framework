using System.IO;

namespace Griffin.Net.Protocols.Stomp
{
    /// <summary>
    ///     A message according to the STOMP specification.
    /// </summary>
    public interface IFrame
    {
        /// <summary>
        ///     A collection of headers
        /// </summary>
        IHeaderCollection Headers { get; }

        /// <summary>
        ///     Application message
        /// </summary>
        Stream Body { get; set; }

        /// <summary>
        ///     Size of the body
        /// </summary>
        int ContentLength { get; set; }

        /// <summary>
        ///     Name of the STOMP frame
        /// </summary>
        string Name { get; }

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