using System.IO;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    /// Serialize or deserialize the object which is transferred.
    /// </summary>
    /// <remarks>
    /// <para>A suggestions is that you use the <c>Type.AssemblyQualifiedName</c> as the content name of your application is .NET only.</para>
    /// <para>The methods must be isolated (i.e. should be able to call the same instance them from several threads).</para>
    /// </remarks>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">If you include the type name to it after the format name, for instance <c>json;YourApp.DTO.User,YourApp</c></param>
        /// <returns>Content name (will be passed to the <see cref="Deserialize"/> method in the other end)</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        void Serialize(object source, Stream destination, out string contentType);

        /// <summary>
        /// Deserialize the content from the stream.
        /// </summary>
        /// <param name="contentType">Used to identify the object which is about to be deserialized. Specified by the <c>Serialize()</c> method when invoked in the other end point.</param>
        /// <param name="source">Stream that contains the object to deserialize.</param>
        /// <returns>Created object</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        object Deserialize(string contentType, Stream source);
    }
}