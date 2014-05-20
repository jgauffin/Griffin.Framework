using System;
using System.IO;
using System.Runtime.Serialization;

namespace Griffin
{
    /// <summary>
    ///     Serialize/deserialize an object
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     Servialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <exception cref="SerializationException">Failed to serialize object</exception>
        void Serialize(object source, Stream destination);

        /// <summary>
        ///     Servialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <param name="baseType">
        ///     If specified, we should be able to differentiate sub classes, i.e. include type information if
        ///     <c>source</c> is of another type than this one.
        /// </param>
        /// <exception cref="SerializationException">Failed to serialize object</exception>
        void Serialize(object source, Stream destination, Type baseType);


        /// <summary>
        ///     Deserialize a stream
        /// </summary>
        /// <param name="source">Stream to read from</param>
        /// <param name="targetType">Type to deserialize. Should be the base type if inheritance is used.</param>
        /// <returns>Serialized object</returns>
        /// <exception cref="SerializationException">Failed to deserialize object</exception>
        object Deserialize(Stream source, Type targetType);
    }
}