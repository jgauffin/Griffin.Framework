using System;

namespace Griffin.Cqs.Http
{
    /// <summary>
    ///     Used to serialize CQS DTOs.
    /// </summary>
    public interface ICqsDeserializer
    {
        /// <summary>
        ///     Deserialize inbound object
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <param name="message">serialized object</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(Type type, string message);

        /// <summary>
        /// Serialize outbound message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="contentType">Content type to specify in outbound header (if any)</param>
        /// <returns>Serialized message</returns>
        string Serialize(object message, out string contentType);
    }
}