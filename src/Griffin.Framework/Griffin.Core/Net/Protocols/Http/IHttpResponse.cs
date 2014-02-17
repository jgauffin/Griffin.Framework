namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     A http response.
    /// </summary>
    public interface IHttpResponse : IHttpMessage
    {
        /// <summary>
        ///     HTTP status code. You typically choose one of <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        int HttpStatusCode { get; set; }

        /// <summary>
        ///     Why the specified <see cref="HttpStatusCode" /> was set.
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