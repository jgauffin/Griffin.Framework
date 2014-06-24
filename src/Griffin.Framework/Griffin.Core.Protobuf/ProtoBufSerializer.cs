using System;
using System.Collections.Concurrent;
using System.IO;
using Griffin.Net.Protocols.Serializers;
using ProtoBuf;

namespace Griffin.Core.Protobuf
{
    /// <summary>
    ///     Allows you to use protobuf-net together with MicroMsg in the networking library.
    /// </summary>
    public class ProtoBufSerializer : IMessageSerializer
    {
        private static readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();
        private static readonly string[] ContentTypes = {"application/protbuf"};

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>application/protobuf;type=YourApp.DTO.User-YourApp</c>
        /// </param>
        public void Serialize(object source, Stream destination, out string contentType)
        {
            Serializer.NonGeneric.Serialize(destination, source);
            contentType = "application/protobuf;" + source.GetType().FullName;
        }

        /// <summary>
        ///     Returns <c>application/protbuf</c>
        /// </summary>
        public string[] SupportedContentTypes
        {
            get { return ContentTypes; }
        }

        /// <summary>
        ///     Deserialize the content from the stream.
        /// </summary>
        /// <param name="contentType">Expects <c>application/protobuf;type=Full.Type.Name</c>.</param>
        /// <param name="source">Stream that contains the object to deserialize.</param>
        /// <returns>
        ///     Created object
        /// </returns>
        /// <exception cref="System.NotSupportedException">Expected protobuf as content type</exception>
        public object Deserialize(string contentType, Stream source)
        {
            Type type;
            if (!_types.TryGetValue(contentType, out type))
            {
                var pos = contentType.IndexOf(";");
                if (pos == -1)
                    throw new NotSupportedException("Expected protobuf");

                type = Type.GetType(contentType.Substring(pos + 1), true);
                _types[contentType] = type;
            }

            return Serializer.NonGeneric.Deserialize(type, source);
        }
    }
}