using System.Collections.Generic;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Collection of cookies
    /// </summary>
    /// <typeparam name="T">Type of cookie</typeparam>
    public interface IHttpCookieCollection<T> : IEnumerable<T> where T : class, IHttpCookie
    {
        /// <summary>
        /// Gets the count of cookies in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the cookie of a given identifier (<c>null</c> if not existing).
        /// </summary>
        T this[string id] { get; }

        /// <summary>
        /// Add a cookie.
        /// </summary>
        /// <param name="cookie">Cookie to add</param>
        void Add(T cookie);

        /// <summary>
        /// Remove all cookies.
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove a cookie from the collection.
        /// </summary>
        /// <param name="cookieName">Name of cookie.</param>
        void Remove(string cookieName);
    }
}