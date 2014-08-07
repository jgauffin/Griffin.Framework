using System;
using System.Collections.Generic;
using System.IO;
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
    public class CompositeIMessageSerializer : IMessageSerializer
    {
        private readonly Dictionary<string, IMessageSerializer> _decoders = new Dictionary<string, IMessageSerializer>();
        private readonly bool _enforceDecoding;

		  /// <summary>
		  ///     Initializes a new instance of the <see cref="CompositeIMessageSerializer" /> class.
		  /// </summary>
        public CompositeIMessageSerializer() : this(true)
		  {
		  }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeIMessageSerializer" /> class.
        /// </summary>
		  /// <param name="enforceDecoding">Do not ignore unknown content types</param>
        public CompositeIMessageSerializer(bool enforceDecoding)
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
        public string[] SupportedContentTypes { get; private set; }

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
            IMessageSerializer decoder;
            var contentTypeTrimmed = GetContentTypeWithoutCharset(contentType);

				if (!_decoders.TryGetValue(contentTypeTrimmed, out decoder))
				{
					if (_enforceDecoding)
						return null;
					else
						return new FormAndFilesResult()
						{
							Files = new Griffin.Net.Protocols.Http.Messages.HttpFileCollection(),
							Form = new Griffin.Net.Protocols.Http.Messages.ParameterCollection()
						};
				}
               

            return decoder.Deserialize(contentType, source);
        }

        /// <summary>
        ///     Add another handlers.
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <param name="decoder">The decoder implementation. Must be thread safe.</param>
        public void Add(string mimeType, IMessageSerializer decoder)
        {
            if (mimeType == null) throw new ArgumentNullException("mimeType");
            if (decoder == null) throw new ArgumentNullException("decoder");
            _decoders[mimeType] = decoder;
        }

        private string GetContentTypeWithoutCharset(string contentType)
        {
            if (!String.IsNullOrEmpty(contentType))
            {
                var pos = contentType.IndexOf(";");

                if (pos > 0)
                {
                    return contentType.Substring(0, pos).Trim();
                }
            }

            return contentType;
        }
    }
}