using System.IO;
using System.Runtime.Serialization;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Serializers
{
    /// <summary>
    ///     Serialize or deserialize messages.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A suggestions is that you use the <c>Type.AssemblyQualifiedName</c> as the content name of your application
    ///         is .NET only.
    ///     </para>
    ///     <para>The methods must be isolated (i.e. should be able to call the same instance them from several threads).</para>
    ///     <para>
    ///         The content type should follow the format defined by the HTTP specification (RFC2616) where the media type can
    ///         be followed by a semicolon and then
    ///         commaseparated key-value pairs. However, we might want to include the .NET type in the message which contains a
    ///         colon between the FullName and the Assembly. In the content type
    ///         we therefore need to replace it with a hypen. See example below
    ///     </para>
    ///     <code>
    /// application/json;type=Your.App.Namespace.SomeType-YourApp
    /// </code>
    ///     <para>
    ///         That allows us to support media types which are used by HTTP and other protocols where other parameters are
    ///         included, for example:
    ///     </para>
    ///     <code>
    /// application/x-www-form-urlencoded;charset=windows-1250;type=Your.App.Namespace.SomeType-YourApp
    /// </code>
    /// <para>
    /// You can use <see cref="HttpHeaderValue"/> if you want to extract the actual content type and it's parameters.
    /// </para>
    /// </remarks>
    public interface IMessageSerializer
    {
        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        string[] SupportedContentTypes { get; }

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
        object Deserialize(string contentType, Stream source);

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>json;type=YourApp.DTO.User-YourApp</c>
        /// </param>
        /// <returns>Content name (will be passed to the <see cref="Deserialize" /> method in the other end)</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        void Serialize(object source, Stream destination, out string contentType);
    }
}