namespace Griffin.Data.Mapper.Values
{
    /// <summary>
    ///     Used by the adapters in <see cref="PropertyMapping{TEntity}" />.
    /// </summary>
    /// <param name="context">Conversion context</param>
    /// <returns>Modified value</returns>
    public delegate object PropertyToColumnValueHandler<TEntity>(PropertyToColumnValueContext<TEntity> context);
}