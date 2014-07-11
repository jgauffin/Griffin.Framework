using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Scans all assemblies in the current <c>AppDomain</c> after types that implement <see cref="ICrudEntityMapper" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All mappers are added as created instances to an internal cache for fast access during mapping operations.
    ///         Hence it's important
    ///         that they are thread safe and considered as singletons when this class is used.
    ///     </para>
    ///     <para>
    ///     </para>
    /// </remarks>
    public class AssemblyScanningMappingProvider : IMappingProvider
    {
        private readonly Dictionary<Type, object> _mappers = new Dictionary<Type, object>();
        private readonly IList<Assembly> _scannedAssemblies = new List<Assembly>();

        /// <summary>
        ///     Ignore mapper classes which are invalid when locating all mappings in the loaded assemblies.
        /// </summary>
        /// <remarks>
        ///     <para>The <c>Scan()</c> method </para>
        /// </remarks>
        public bool IgnoreInvalidMappers { get; set; }

        /// <summary>
        ///     If <c>true</c>, we'll replace the first mapper if we encounter a second mapper for the same entity.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <c>false</c> means that an exception will be thrown
        ///     </para>
        /// </remarks>
        public bool ReplaceDuplicateMappers { get; set; }

        /// <summary>
        ///     Scan has been called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Used by the library to check if the library should trigger a scan.
        ///     </para>
        /// </remarks>
        public bool HasScanned { private set; get; }

        /// <summary>
        ///     Retrieve a mapper.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to retrieve a mapper for.</typeparam>
        /// <returns>Mapper</returns>
        /// <exception cref="MappingNotFoundException">Failed to find a mapping for the given entity type.</exception>
        /// <remarks>
        ///     <para>
        ///         Do note that the mapper must implement <see cref="ICrudEntityMapper{TEntity}" /> interface for this method to work.
        ///     </para>
        /// </remarks>
        public ICrudEntityMapper Get<TEntity>()
        {
            object mapper;
            if (!_mappers.TryGetValue(typeof (TEntity), out mapper))
            {
                if (!_mappers.TryGetValue(typeof (TEntity), out mapper))
                    throw new MappingNotFoundException(typeof (TEntity));
            }

            var genericMapper = mapper as ICrudEntityMapper<TEntity>;
            if (genericMapper == null)
                throw new MappingException(typeof (TEntity),
                    "The mapper for '" + typeof (TEntity).FullName +
                    "' must implement the generic interface 'ICrudEntityMapper<T>' for this method to work.");

            return genericMapper;
        }

        /// <summary>
        ///     Get mapping for the specified entity type
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns>Mapper</returns>
        /// <exception cref="NotSupportedException">The specified entity type is not supported.</exception>
        public IEntityMapper GetBase<TEntity>()
        {
            object mapper;
            if (!_mappers.TryGetValue(typeof (TEntity), out mapper))
            {
                if (!_mappers.TryGetValue(typeof (TEntity), out mapper))
                    throw new MappingNotFoundException(typeof (TEntity));
            }

            var genericMapper = mapper as IEntityMapper<TEntity>;
            if (genericMapper == null)
                throw new MappingException(typeof (TEntity),
                    "The mapper for '" + typeof (TEntity).FullName +
                    "' must implement the generic interface 'IEntityMapper<T>' for this method to work.");

            return genericMapper;
        }

        /// <summary>
        ///     Retrieve a mapper.
        /// </summary>
        /// <typeparam name="T">Type of entity to retrieve a mapper for.</typeparam>
        /// <param name="entityType">Type of entity to get a mapper for</param>
        /// <returns>Mapper</returns>
        /// <exception cref="MappingNotFoundException">Failed to find a mapping for the given entity type.</exception>
        public ICrudEntityMapper Get(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException("entityType");

            object mapper;
            if (!_mappers.TryGetValue(entityType, out mapper))
            {
                if (!_mappers.TryGetValue(entityType, out mapper))
                    throw new MappingNotFoundException(entityType);
            }

            return (ICrudEntityMapper) mapper;
        }

        /// <summary>
        ///     Scan all loaded assemblies in the current domain.
        /// </summary>
        public void Scan()
        {
            HasScanned = true;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly == Assembly.GetExecutingAssembly())
                    continue;
                if (assembly.IsDynamic)
                    continue;

                Scan(assembly);
            }
        }

        /// <summary>
        ///     Scan all loaded assemblies in the current domain.
        /// </summary>
        /// <param name="assembly">Assembly to scan for types that implement <see cref="ICrudEntityMapper" />.,</param>
        public void Scan(Assembly assembly)
        {
            if (_scannedAssemblies.Contains(assembly))
                return;

            HasScanned = true;
            var types = assembly.GetTypes().Where(x => typeof (ICrudEntityMapper).IsAssignableFrom(x));
            foreach (var type in types)
            {
                Console.WriteLine("scanning: " + type.FullName);

                if (type.IsAbstract || type.IsInterface)
                    continue;

                object instance;
                Type entityType;
                if (type.GetGenericArguments().Length > 0)
                    continue;

                Console.WriteLine("adding: " + type.FullName);
                if (type.BaseType != null && type.BaseType.GenericTypeArguments.Length == 1)
                {
                    entityType = type.BaseType.GenericTypeArguments[0];
                    instance = CreateInstance(type);
                }
                else
                {
                    var attr = type.GetCustomAttribute<MappingForAttribute>();
                    if (attr != null)
                    {
                        instance = CreateInstance(type);
                        entityType = attr.EntityType;
                    }
                    else
                    {
                        var genericInterface = type.GetInterface("ICrudEntityMapper`1");
                        if (genericInterface == null)
                        {
                            if (IgnoreInvalidMappers)
                                continue;

                            throw new MappingException(type,
                                "Entity mappers must either implement ICrudEntityMapper<T> or be decorated with the [MappingFor(typeof(YourEntity))] attribute. Adjust '" +
                                type.FullName + "' accordingly.");
                        }


                        entityType = genericInterface.GenericTypeArguments[0];
                        instance = CreateInstance(type);
                    }
                }


                if (_mappers.ContainsKey(entityType) && !ReplaceDuplicateMappers)
                    throw new MappingException(entityType,
                        string.Format("Two mappers found for entity '{0}'. '{1}' and '{2}'. Remove one of them.",
                            entityType.FullName, type.FullName, _mappers[entityType].GetType().FullName));

                ((ICrudEntityMapper) instance).Freeze();
                _mappers[entityType] = instance;
            }
        }

        private static object CreateInstance(Type type)
        {
            object instance;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (MissingMethodException exception)
            {
                throw new MappingException(type, "All mappers must have a default constructor (can be non public).",
                    exception);
            }
            catch (Exception exception)
            {
                throw new MappingException(type,
                    "Failed to initialize mapper class. Is the mappings correct? Check inner exception for more information.",
                    exception);
            }
            return instance;
        }
    }
}