namespace Griffin.Data.Mapper
{
    internal static class MappingExtensions
    {
        public static ICrudEntityMapper<TEntity> AsCrudMapper<TEntity>(this IEntityMapper mapper)
        {
            var crudMapper = mapper as ICrudEntityMapper<TEntity>;
            if (crudMapper == null)
            {
                throw new MappingException(typeof(TEntity),
                    $"Mapper for {typeof(TEntity)} must be of type ICrudMapper<T>.");
            }

            return crudMapper;
        }

        public static ICrudEntityMapper<TEntity> AsCrudMapper<TEntity>(this IEntityMapper<TEntity> mapper)
        {
            var crudMapper = mapper as ICrudEntityMapper<TEntity>;
            if (crudMapper == null)
            {
                throw new MappingException(typeof(TEntity), "To use partial SQL, you need to implement ICrudEntityMapper<T> for entity '" + typeof(TEntity) + "'.");
            }

            return crudMapper;
        }
    }
}