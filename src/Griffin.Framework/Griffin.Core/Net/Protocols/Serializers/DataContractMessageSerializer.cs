using System;
using System.IO;
using System.Runtime.Serialization;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Serializers
{
    /// <summary>
    ///     Uses the DataContract XML serializer.
    /// </summary>
    public class DataContractMessageSerializer : IMessageSerializer
    {
        /// <summary>
        ///     <c>application/x-datacontract</c>
        /// </summary>
        public const string MimeType = "application/x-datacontract";

        public void Serialize(object source, Stream destination, out string contentType)
        {
            var serializer = new DataContractSerializer(source.GetType());
            serializer.WriteObject(destination, source);
            contentType = string.Format("{0};type={1}", MimeType,
                source.GetType().GetSimplifiedAssemblyQualifiedName().Replace(',', '-'));
        }

        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes { get; private set; }

        public object Deserialize(string contentType, Stream source)
        {
            if (contentType == null) throw new ArgumentNullException("contentType");
            var header = new HttpHeaderValue(contentType);

            // to be backwards compatible.
            Type type;
            var typeArgument = header.Parameters["type"];
            if (typeArgument == null)
            {
                var pos = contentType.IndexOf(';');
                if (pos == -1)
                {
                    type = Type.GetType(contentType, false, true);
                }
                else
                {
                    type = Type.GetType(contentType.Substring(pos + 1), false, true);
                }
            }
            else
            {
                var typeName = header.Parameters["type"].Replace("-", ",");
                type = Type.GetType(typeName, true);
            }

            if (type == null)
                throw new FormatException("Failed to build a type from '" + contentType + "'.");


            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(source);
        }
    }
}