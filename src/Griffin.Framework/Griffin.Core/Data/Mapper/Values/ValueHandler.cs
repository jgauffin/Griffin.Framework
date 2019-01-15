namespace Griffin.Data.Mapper.Values
{
    /// <summary>
    ///     Used by the adapters in <see cref="PropertyMapping{TEntity}" />.
    /// </summary>
    /// <param name="originalValue">Value from column or property depending on the mapping direction</param>
    /// <returns>Modified value</returns>
    public delegate object ValueHandler(object originalValue);
}