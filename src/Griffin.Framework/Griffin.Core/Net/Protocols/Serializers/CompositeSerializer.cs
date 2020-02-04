using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Griffin.Net.Protocols.Http.Serializers;

namespace Griffin.Net.Protocols.Serializers
{
    /// <summary>
    ///     Can provide one or more decoders.
    /// </summary>
    /// <remarks>
    ///     The default implementation constructor uses <see cref="UrlFormattedMessageSerializer" /> and
    ///     <see cref="MultipartSerializer" />
    /// </remarks>
    public class CompositeMessageSerializer : IMessageSerializer
    {
        private readonly Dictionary<string, IMessageSerializer>
            _decoders = new Dictionary<string, IMessageSerializer>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeMessageSerializer" /> class.
        /// </summary>
        public CompositeMessageSerializer()
        {
            _decoders.Add(UrlFormattedMessageSerializer.MimeType, new UrlFormattedMessageSerializer());
            _decoders.Add(MultipartSerializer.MimeType, new MultipartSerializer());
            DefaultEncoding = Encoding.UTF8;
        }

        /// <summary>
        ///     Encoding to use if not specified in the HTTP request.
        /// </summary>
        public Encoding DefaultEncoding { get; set; }

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>json;YourApp.DTO.User,YourApp</c>
        /// </param>
        /// <returns>Content name (will be passed to the <see cref="IMessageSerializer.Deserialize" /> method in the other end)</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public void Serialize(object source, Stream destination, out string contentType)
        {
            throw new NotSupportedException("We can currently not encode outbound message bodies.");
        }

        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes => _decoders.Keys.ToArray();

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
            var contentTypeTrimmed = GetContentTypeWithoutCharset(contentType);

            return _decoders.TryGetValue(contentTypeTrimmed, out var decoder)
                ? decoder.Deserialize(contentType, source)
                : null;
        }

        /// <summary>
        ///     Add another handlers.
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <param name="decoder">The decoder implementation. Must be thread safe.</param>
        public void Add(string mimeType, IMessageSerializer decoder)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));
            _decoders[mimeType] = decoder ?? throw new ArgumentNullException(nameof(decoder));
        }

        /// <summary>
        ///     Remove all serializers = allow all content types.
        /// </summary>
        public void Clear()
        {
            _decoders.Clear();
        }

        private string GetContentTypeWithoutCharset(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return contentType;

            var pos = contentType.IndexOf(";", StringComparison.Ordinal);
            return pos > 0
                ? contentType.Substring(0, pos).Trim()
                : contentType;
        }
    }
}