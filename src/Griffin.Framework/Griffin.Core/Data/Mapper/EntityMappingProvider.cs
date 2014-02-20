using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Facade for the current <see cref="IMappingProvider" /> implementation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This facade exists so that no code needs to be refactored if the mapping provider is replace with another
    ///         one.
    ///     </para>
    ///     <para>
    ///         The <see cref="AssemblyScanningMappingProvider" /> is used per default (if no other provider is assigned). It
    ///         is lazy loaded so that
    ///         all assemblies have a chance to be loaded before it scans all assemblies in the current appdomain.
    ///     </para>
    /// </remarks>
    public class EntityMappingProvider
    {
        private static IMappingProvider _provider = new AssemblyScanningMappingProvider();
        private static bool _scanned = false;

        /// <summary>
        ///     Provider to use.
        /// </summary>
        /// <value>
        ///     Default is <see cref="AssemblyScanningMappingProvider" />. Read the class remarks for more information.
        /// </value>
        public static IMappingProvider Provider
        {
            get { return _provider; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _provider = value;
            }
        }

        /// <summary>
        ///     Get a mapper.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to get a mapper for.</typeparam>
        /// <exception cref="MappingNotFoundException">Did not find a mapper for the specified entity.</exception>
        /// <returns></returns>
        public static IEntityMapper<TEntity> GetMapper<TEntity>()
        {
            if (!_scanned && _provider is AssemblyScanningMappingProvider)
            {
                _scanned = true;
                ((AssemblyScanningMappingProvider) _provider).Scan();
            }

            var mapper= (IEntityMapper<TEntity>) _provider.Get<TEntity>();
            return mapper;
        }
    }
}