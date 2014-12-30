using System.Collections.Generic;

namespace Griffin.Net.Protocols.Stomp
{
    /// <summary>
    /// Collection of STOMP headers
    /// </summary>
    public interface IHeaderCollection : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Amount of headers
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Access the value of a header.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        string this[string name] { get; set; }

        /// <summary>
        /// Checks if the collection contains the specified header.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        bool Contains(string name);
    }
}