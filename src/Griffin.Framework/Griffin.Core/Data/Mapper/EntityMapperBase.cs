using System.Collections.Generic;
using System.Data;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Used to map a <see cref="IDataRecord" /> to an entity.
    /// </summary>
    /// <remarks>
    ///     <para>Just hides the non generic methods from the public contract.</para>
    /// </remarks>
    public abstract class EntityMapperBase<TEntity> : IEntityMapper<TEntity>
    {
        /// <summary>
        ///     Create a new entity for the specified
        /// </summary>
        /// <param name="record">Data record that we are going to map</param>
        /// <returns>Created entity</returns>
        /// <remarks>
        ///     <para>
        ///         The provided record should only be used if there are constructor arguments.
        ///     </para>
        /// </remarks>
        public abstract TEntity Create(IDataRecord record);

        /// <summary>
        ///     Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        void IEntityMapper.Map(IDataRecord source, object destination)
        {
            Map(source, (TEntity)destination);
        }

        /// <summary>
        /// Free the mapping, no further changes may be made.
        /// </summary>
        /// <remarks>
        /// <para>Called by the mapping provider when it's being added to it.</para>
        /// </remarks>
        public abstract void Freeze();

        /// <summary>
        /// Gets table name
        /// </summary>
        public abstract string TableName { get; }

        /// <summary>
        /// All properties in this mapping
        /// </summary>
        public abstract IDictionary<string, IPropertyMapping> Properties { get; }

        /// <summary>
        /// Get the primary key 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>A single item in the array for a single PK column and one entry per column in composite primary key</returns>
        public abstract KeyValuePair<string, object>[] GetKeys(object entity);

        /// <summary>
        /// Used to create SQL commands which is specific for this entity.
        /// </summary>
        /// <remarks>
        /// <para>The recommended approach for implementations is to retrieve the command builder from <see cref="CommandBuilderFactory"/> when the <c>Freeze()</c> method is being invoked.
        /// By doing so it's easy to adapt and precompile the command strings and logic before any invocations is made.
        /// </para>
        /// </remarks>
        public abstract ICommandBuilder CommandBuilder { get; }

        /// <summary>
        ///     Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        public abstract void Map(IDataRecord source, TEntity destination);

        /// <summary>
        ///     Create a new entity for the specified
        /// </summary>
        /// <param name="record">Data record that we are going to map</param>
        /// <returns>Created entity</returns>
        /// <remarks>
        ///     <para>
        ///         The provided record should only be used if there are constructor arguments.
        ///     </para>
        /// </remarks>
        object IEntityMapper.Create(IDataRecord record)
        {
            return Create(record);
        }
    }
}