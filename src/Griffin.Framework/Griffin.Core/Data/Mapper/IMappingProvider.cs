using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Used by the <see cref="MappingProvider"/>
    /// </summary>
    public interface IMappingProvider
    {
        /// <summary>
        /// Get mapping for the specified entity type
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns>Mapper</returns>
        /// <exception cref="NotSupportedException">The specified entity type is not supported.</exception>
        IEntityMapper Get<T>();

    }
}