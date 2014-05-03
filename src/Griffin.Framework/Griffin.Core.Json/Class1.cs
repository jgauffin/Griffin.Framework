using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Griffin.Core.Json
{
    public class JsonSerializer : ISerializer
    {
        private JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            };
        }
        public void Serialize(object source, Stream destination)
        {
            throw new NotImplementedException();
        }

        public void Serialize(object source, Stream destination, Type baseType)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(Stream source, Type targetType)
        {
            throw new NotImplementedException();
        }
    }
}
