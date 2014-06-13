using System.IO;
using System.Runtime.Serialization;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    public class StringSerializer : IMessageSerializer
    {
        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes { get { return new []
        {
            "text/plain"
        }
            ; }}

        /// <summary>
        ///     Deserialize the content from the stream.
        /// </summary>
        /// <param name="contentType">
        ///     Used to identify the object which is about to be deserialized. Specified by the
        ///     <c>Serialize()</c> method when invoked in the other end point.
        /// </param>
        /// <param name="source">Stream that contains the object to deserialize.</param>
        /// <returns>Created object</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public object Deserialize(string contentType, Stream source)
        {
            var reader = new StreamReader(source);
            return reader.ReadToEnd();
        }

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>json;type=YourApp.DTO.User-YourApp</c>
        /// </param>
        /// <returns>Content name (will be passed to the <see cref="IMessageSerializer.Deserialize" /> method in the other end)</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public void Serialize(object source, Stream destination, out string contentType)
        {
            var writer = new StreamWriter(destination);
            writer.Write(source);
            contentType = "text/plain";
        }
    }
}