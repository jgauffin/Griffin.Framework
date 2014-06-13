using System;
using System.Security.Cryptography.X509Certificates;
using Griffin.Cqs.Net.Modules;
using Griffin.Net.Protocols;
using Griffin.Net.Server.Modules;

namespace Griffin.Net.Server
{
    /// <summary>
    /// Configuration for <see cref="LiteServer"/>
    /// </summary>
    public class LiteServerConfiguration
    {
        /// <summary>
        /// To be able to encode wrapper objects (for instance to encode a HTTP response into <c>byte[]</c>).
        /// </summary>
        /// <example>
        /// <code>
        /// config.EncoderFactory = () => new HttpEncoderFactory(
        /// </code>
        /// </example>
        public Func<IMessageEncoder> EncoderFactory { get; set; }

        /// <summary>
        /// To be able to decode <c>byte[]</c> into wrapper objects (like a HTTP request)
        /// </summary>
        public Func<IMessageDecoder> DecoderFactory { get; set; }

        /// <summary>
        /// All modules that should be executed for every incoming message
        /// </summary>
        public ModulePipeBuilder Modules { get; set; }

        /// <summary>
        /// If we want to have secure communication.
        /// </summary>
        public X509Certificate Certificate { get; set; }

    }
}