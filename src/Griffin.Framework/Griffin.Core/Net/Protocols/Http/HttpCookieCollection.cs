using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// A collection of HTTP cookies
    /// </summary>
    /// <typeparam name="T">Type of cookie</typeparam>
    public class HttpCookieCollection<T> : IHttpCookieCollection<T> where T : class, IHttpCookie
    {
        private readonly List<T> _items = new List<T>();

        #region IHttpCookieCollection<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds the specified cookie.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        public void Add(T cookie)
        {
            if (cookie == null)
                throw new ArgumentNullException("cookie");

            _items.Add(cookie);
        }

        /// <summary>
        /// Gets the count of cookies in the collection.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Gets the cookie of a given identifier (<c>null</c> if not existing).
        /// </summary>
        public T this[string id]
        {
            get
            {
                if (id == null) throw new ArgumentNullException("id");
                return _items.FirstOrDefault(x => x.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Remove all cookies.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Remove a cookie from the collection.
        /// </summary>
        /// <param name="cookieName">Name of cookie.</param>
        public void Remove(string cookieName)
        {
            if (cookieName == null) throw new ArgumentNullException("cookieName");
            _items.RemoveAll(x => x.Name.Equals(cookieName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}