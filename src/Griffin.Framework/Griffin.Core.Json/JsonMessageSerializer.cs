using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Griffin.Net.Protocols.MicroMsg;
using Newtonsoft.Json;

namespace Griffin.Core.Json
{
    /// <summary>
    /// Serializer for the <see cref="Griffin.Net.Protocols.MicroMsg"/> protocol using JSON.NET.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All objects are serialized using <c>typeof(object)</c> as type, which forces JSON.NET to include the type name
    ///         in every created JSON document. In this way you will
    ///         never get any problem with inheritance etc.
    ///     </para>
    /// </remarks>
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _settings;
        private static ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonMessageSerializer" /> class.
        /// </summary>
        public JsonMessageSerializer()
        {
            _settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            };

            Encoding = Encoding.UTF8;
        }


        /// <summary>
        ///     Encoding used to read/write JSON
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>application/json;YourApp.DTO.User,YourApp</c>
        /// </param>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public void Serialize(object source, Stream destination, out string contentType)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            serializer.Serialize(new StreamWriter(destination, Encoding), source);
            contentType = "application/json;" + source.GetType().GetSimplifiedAssemblyQualifiedName();
        }

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
            Type type;
            if (!_types.TryGetValue(contentType, out type))
            {
                int pos = contentType.IndexOf(";");
                if (pos == -1)
                    throw new NotSupportedException("Expected protobuf");

                type = Type.GetType(contentType.Substring(pos + 1), true);
                _types[contentType] = type;
            }

            var serializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            return serializer.Deserialize(new StreamReader(source, Encoding), type);
        }
    }
}