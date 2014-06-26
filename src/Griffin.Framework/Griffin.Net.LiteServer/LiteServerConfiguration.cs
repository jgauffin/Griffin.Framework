using System;
using System.Security.Cryptography.X509Certificates;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net.LiteServer
{
    /// <summary>
    ///     Configuration for <see cref="LiteServer" />
    /// </summary>
    public class LiteServerConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteServerConfiguration"/> class.
        /// </summary>
        public LiteServerConfiguration()
        {
            EncoderFactory = () => new MicroMessageEncoder(new DataContractMessageSerializer());
            DecoderFactory = () => new MicroMessageDecoder(new DataContractMessageSerializer());
            Modules = new ModulePipeBuilder();
        }

        /// <summary>
        ///     To be able to encode wrapper objects (for instance to encode a HTTP response into <c>byte[]</c>).
        /// </summary>
        /// <example>
        ///     <code>
        /// config.EncoderFactory = () => new HttpEncoderFactory(
        /// </code>
        /// </example>
        public Func<IMessageEncoder> EncoderFactory { get; set; }

        /// <summary>
        ///     To be able to decode <c>byte[]</c> into wrapper objects (like a HTTP request)
        /// </summary>
        public Func<IMessageDecoder> DecoderFactory { get; set; }

        /// <summary>
        ///     All modules that should be executed for every incoming message
        /// </summary>
        public ModulePipeBuilder Modules { get; private set; }

        /// <summary>
        ///     If we want to have secure communication.
        /// </summary>
        public X509Certificate Certificate { get; set; }
    }
}