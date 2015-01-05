using System;
using System.Collections;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Stomp.Frames
{
    /// <summary>
    ///     Collection of STOMP headers
    /// </summary>
    /// <remarks>The values are not encoded, you must encode them when serializing the message.</remarks>
    public class HeaderCollection : IHeaderCollection
    {
        private readonly Dictionary<string, string> _items = new Dictionary<string, string>();

        /// <summary>
        /// Amount of headers
        /// </summary>
        public int Count { get { return _items.Count; } }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public string this[string name]
        {
            get
            {
                string value;
                if (!_items.TryGetValue(name.ToLower(), out value))
                    return null;

                return value;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _items[name.ToLower()] = value;
            }
        }

   
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
        public void Add(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (name.IndexOfAny(new[] {':', '\r', '\n'}) != -1)
                throw new FormatException("Header name may not contain colon, CR or LF; Name: " + name);
            if (value != null && value.IndexOfAny(new[] {':', '\r', '\n'}) != -1)
                throw new FormatException("Header value may not contain colon, CR or LF; Value: " + value);
            name = name.ToLower();

            if (_items.ContainsKey(name))
                return;

            _items[name] = value;
        }

        /// <summary>
        /// Checks if the collection contains the specified header.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns>
        ///   <c>true</c> if found; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(string name)
        {
            return _items.ContainsKey(name);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}