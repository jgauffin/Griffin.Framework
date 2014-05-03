using System;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Create a new HTTP cookie
    /// </summary>
    /// <remarks>Typically a request cookie, since response cookies need more information.</remarks>
    /// <seealso cref="HttpResponseCookie"/>
    public class HttpCookie : IHttpCookie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCookie" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public HttpCookie(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            Name = name;
            Value = value;
        }

        #region IHttpCookie Members

        /// <summary>
        /// Gets the cookie identifier.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets value. 
        /// </summary>
        public string Value { get; private set; }

        #endregion
    }
}