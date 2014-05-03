using System;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Response cookies also have an expiration and the path that they are valid for.
    /// </summary>
    public class HttpResponseCookie : HttpCookie, IResponseCookie
    {
        #region IResponseCookie Members

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseCookie" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public HttpResponseCookie(string name, string value)
            : base(name, value)
        {
        }

        /// <summary>
        /// Gets domain that the cookie is valid under
        /// </summary>
        /// <remarks><c>null</c> means not specified</remarks>
        public string Domain { get; set; }

        /// <summary>
        /// Gets when the cookie expires.
        /// </summary>
        /// <remarks><see cref="DateTime.MinValue"/> means that the cookie expires when the session do so.</remarks>
        public DateTime ExpiresUtc { get; set; }

        /// <summary>
        /// Gets path that the cookie is valid under.
        /// </summary>
        /// <remarks><c>null</c> means not specified.</remarks>
        public string Path { get; set; }

        #endregion

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var r = string.Format("{0}={1}", Name, Value);
            if (Domain != null)
                r += "; Domain=" + Domain;
            if (ExpiresUtc != DateTime.MinValue)
                r += "; Expires=" + ExpiresUtc.ToString("R");
            if (Path != null)
                r += "; Path=" + Path;

            return r;
        }
    }
}