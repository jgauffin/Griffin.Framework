using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;

namespace Griffin.Core.Json
{
    /// <summary>
    /// JsonSerializer using JSON.NET
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
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
        /// Encoding used to read/write JSON 
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Servialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        public void Serialize(object source, Stream destination)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            serializer.Serialize(new StreamWriter(destination), source);
        }

        /// <summary>
        /// Servialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <param name="baseType">If specified, we should be able to differentiate sub classes, i.e. include type information if
        /// <c>source</c> is of another type than this one.</param>
        public void Serialize(object source, Stream destination, Type baseType)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            serializer.Serialize(new StreamWriter(destination), source, baseType);
        }

        /// <summary>
        /// Deserialize a stream
        /// </summary>
        /// <param name="source">Stream to read from</param>
        /// <param name="targetType">Type to deserialize. Should be the base type if inheritance is used.</param>
        /// <returns>
        /// Serialized object
        /// </returns>
        public object Deserialize(Stream source, Type targetType)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            return serializer.Deserialize(new StreamReader(source, Encoding), targetType);
        }
    }
}