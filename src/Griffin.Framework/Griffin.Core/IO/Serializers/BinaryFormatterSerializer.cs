using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Runtime.Remoting.Messaging; TODO: check - Looks like this namespace is not used
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.IO.Serializers
{
    /// <summary>
    /// Wraps around <see cref="BinaryFormatter"/>.
    /// </summary>
    public class BinaryFormatterSerializer : ISerializer
    {
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        public void Serialize(object source, Stream destination)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(destination, source);
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <param name="baseType">If specified, we should be able to differentiate sub classes, i.e. include type information if
        /// <c>source</c> is of another type than this one.</param>
        public void Serialize(object source, Stream destination, Type baseType)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(destination, source);
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
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(source);
        }
    }
}
