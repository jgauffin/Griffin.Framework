namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     A http response.
    /// </summary>
    public interface IHttpResponse : IHttpMessage
    {
        /// <summary>
        ///     Cookies to send to the server side
        /// </summary>
        IHttpCookieCollection<IResponseCookie> Cookies { get; }

        /// <summary>
        ///     HTTP status code. You typically choose one of <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        ///     Why the specified <see cref="StatusCode" /> was set.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The goal with the reason is to help the remote endpoint to understand why the specific code was chosen. i.e. it
        ///         allows you
        ///         to help the programmer to understand why a specific error code was set.
        ///     </para>
        /// </remarks>
        string ReasonPhrase { get; set; }
    }
}