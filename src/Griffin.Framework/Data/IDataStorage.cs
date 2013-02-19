namespace Griffin.Framework.Data
{
    /// <summary>
    /// Data storage
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    /// <typeparam name="TKey">Type of primary key</typeparam>
    /// <remarks>You typically build repositories on top of this class (or use them for access in commands if you use the CQS Pattern).</remarks>
    public interface IDataStorage<TEntity, in TKey> where TEntity : class
    {
        /// <summary>
        /// Gets entity
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <returns>Entity if found; otherwise <c>null</c>.</returns>
        TEntity Load(TKey id);

        /// <summary>
        /// Save an entity
        /// </summary>
        /// <param name="entity">Entity to save</param>
        /// <remarks>New items will be created while existing items will be updated.</remarks>
        void Save(TEntity entity);

        /// <summary>
        /// Remove an entity.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        /// <remarks>Removing an non existant item won't do anything (i.e. no exceptions will be thrown)</remarks>
        void Delete(TEntity entity);
    }
}
