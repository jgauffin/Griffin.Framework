using System.Collections.Generic;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Represents all headers that are being sent/received in a HTTP message
    /// </summary>
    public interface IHeaderCollection : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Number of headers
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get or set an header
        /// </summary>
        /// <param name="name">Name, case insensitive</param>
        /// <returns>
        /// Header if found; otherwise <c>null</c>.
        /// </returns>
        string this[string name] { get; set; }

        /// <summary>
        /// Checks if the specified header exists in the collection
        /// </summary>
        /// <param name="name">Name, case insensitive</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        bool Contains(string name);
    }
}