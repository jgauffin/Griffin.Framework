namespace Griffin.Data.Mapper.Values
{
    /// <summary>
    /// Used by <see cref="PropertyToColumnValueHandler{TEntity}"/>.
    /// </summary>
    public class PropertyToColumnValueContext<TEntity>
    {
        public PropertyToColumnValueContext(object value, TEntity entity)
        {
            Value = value;
            Entity = entity;
        }

        /// <summary>
        /// Column value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Entire record
        /// </summary>
        public TEntity Entity { get; private set; }
    }
}