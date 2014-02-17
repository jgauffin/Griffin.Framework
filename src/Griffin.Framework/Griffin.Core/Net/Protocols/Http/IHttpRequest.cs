using System;

namespace Griffin.Net.Protocols.Http
{
    public interface IHttpRequest : IHttpMessage
    {
        /// <summary>
        ///     Method which was invoked.
        /// </summary>
        /// <remarks>
        ///     <para>Typically <c>GET</c>, <c>POST</c>, <c>PUT</c>, <c>DELETE</c> or <c>HEAD</c>.</para>
        /// </remarks>
        string HttpMethod { get; }

        /// <summary>
        ///     Request UrI
        /// </summary>
        /// <remarks>
        ///     <para>Is built using the <c>server</c> header and the path + query which is included in the request line</para>
        ///     <para>If no <c>server</c> header is included "127.0.0.1" will be used as server.</para>
        /// </remarks>
        Uri Uri { get; set; }
    }
}