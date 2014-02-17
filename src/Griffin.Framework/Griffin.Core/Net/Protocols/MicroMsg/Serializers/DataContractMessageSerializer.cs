using System;
using System.IO;
using System.Runtime.Serialization;

namespace Griffin.Net.Protocols.MicroMsg.Serializers
{
    /// <summary>
    /// Uses the DataContract XML serializer.
    /// </summary>
    public class DataContractMessageSerializer : IMessageSerializer
    {

        public void Serialize(object source, Stream destination, out string contentType)
        {
            var serializer = new DataContractSerializer(source.GetType());
            serializer.WriteObject(destination, source);
            contentType = source.GetType().AssemblyQualifiedName;
        }

        public object Deserialize(string contentType, Stream source)
        {
            var type = Type.GetType(contentType, true);
            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(source);
        }
    }
}
