using System;
using System.IO;
using System.Runtime.Serialization;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Serializers
{
    /// <summary>
    /// Uses the DataContract XML serializer.
    /// </summary>
    public class DataContractMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// <c>application/x-datacontract</c>
        /// </summary>
        public const string MimeType = "application/x-datacontract";

        public void Serialize(object source, Stream destination, out string contentType)
        {
            var serializer = new DataContractSerializer(source.GetType());
            serializer.WriteObject(destination, source);
            contentType = string.Format("{0};{1}", MimeType, source.GetType().AssemblyQualifiedName);
        }

        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes { get; private set; }

        public object Deserialize(string contentType, Stream source)
        {
            var header = new HttpHeaderValue(contentType);

            var typeName = header.Parameters["type"].Replace("-", ",");
            var type = Type.GetType(typeName, true);
            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(source);
        }
    }
}
