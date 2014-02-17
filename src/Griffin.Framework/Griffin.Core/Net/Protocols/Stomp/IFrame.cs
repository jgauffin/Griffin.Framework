using System.IO;

namespace Griffin.Net.Protocols.Stomp
{
    public interface IFrame
    {
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
        ///     <para>If a client or a server receives repeated frame header entries, only the first header entry SHOULD be used as the value of header entry. Subsequent values are only used to maintain a history of state changes of the header and MAY be ignored. This implementation will IGNORE all subsequent headers</para>
        /// </remarks>
        void AddHeader(string name, string value);

        IHeaderCollection Headers { get; }

        Stream Body { get; set; }

        int ContentLength { get; set; }
        string Name { get; }
    }
}
