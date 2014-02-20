namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Used by the <see cref="MappingProvider"/>
    /// </summary>
    public interface IMappingProvider
    {
        IEntityMapper Get<T>();

    }
}