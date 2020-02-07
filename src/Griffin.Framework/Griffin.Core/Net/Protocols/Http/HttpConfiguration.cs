using System.Security.Cryptography.X509Certificates;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    ///     Configuration for <see cref="HttpServer" />.
    /// </summary>
    public class HttpConfiguration
    {
        /// <summary>
        ///     Certificate for HTTPs.
        /// </summary>
        public X509Certificate Certificate { get; set; }

        /// <summary>
        ///     Used to be able to deserialize different content types.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Multipart and url formatted content is supported out of the box.
        ///     </para>
        /// </remarks>
        public CompositeMessageSerializer ContentSerializers { get; } = new CompositeMessageSerializer();

        public MessagingServerPipeline<HttpContext> Pipeline { get; } = new MessagingServerPipeline<HttpContext>();

        /// <summary>
        ///     Server port, default = 8080
        /// </summary>
        /// <remarks>
        ///     <para>You must set this value to -1 to disable plain http.</para>
        /// </remarks>
        public int Port { get; set; } = 8080;

        /// <summary>
        ///     Port when using a certificate. Default = 44343
        /// </summary>
        public int SecurePort { get; set; } = 44343;
    }
}