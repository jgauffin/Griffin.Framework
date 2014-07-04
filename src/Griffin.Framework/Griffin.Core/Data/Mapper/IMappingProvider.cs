using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Used by the <see cref="EntityMappingProvider"/>
    /// </summary>
    public interface IMappingProvider
    {
        /// <summary>
        ///     Retrieve a mapper.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to retrieve a mapper for.</typeparam>
        /// <returns>Mapper</returns>
        /// <exception cref="MappingNotFoundException">Failed to find a mapping for the given entity type.</exception>
        /// <remarks>
        /// <para>
        /// Do note that the mapper must implement <see cref="IEntityMapper{TEntity}"/> interface for this method to work.
        /// </para>
        /// </remarks>
        IEntityMapper Get<TEntity>();

        /// <summary>
        /// Get mapping for the specified entity type
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns>Mapper</returns>
        /// <exception cref="MappingNotFoundException">Failed to find a mapping for the given entity type.</exception>
        IEntityMapperBase GetBase<T>();
    }
}