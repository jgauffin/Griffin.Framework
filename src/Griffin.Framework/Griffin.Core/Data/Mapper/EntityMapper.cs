using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Uses reflection to map entities.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity (i.e. class that somewhat corresponds to a table)</typeparam>
    /// <remarks>
    /// <para>
    /// This mapper is konventional based. If there is a column named <c>"Id"</c> this mapper will assume that that is the primary key. If you do not have
    /// an <c>"Id"</c> id column you need to inherit this class and overide the <c>Configure method:</c>
    /// </para>
    /// <code>
    /// <![CDATA[
    /// public class UserMapping : ReflectionBasedEntityMapper<User>
    /// {
    ///     public override void Configure(IDictionary<string, PropertyMapping> mappings)
    ///     {
    ///         base.Configure(mappings);
    /// 
    ///         mappings["YourCustomKey"].IsPrimaryKey = true;
    ///     }
    /// }
    /// ]]>
    /// </code>
    ///     <para>
    ///         All mappers must have a parameterless constructor, but you can set it as non-public if you do not want to expose
    ///         it.
    ///     </para>
    ///     <para>
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         You can just create an empty class like below if there is an one-one mapping between the table and your entity class. It will
    ///         automatically be
    ///         picked up by the <see cref="AssemblyScanningMappingProvider" />.
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : ReflectionBasedEntityMapper<User>
    /// {
    /// }
    /// ]]>
    /// </code>
    ///     <para>You can also customize the mappings</para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : ReflectionBasedEntityMapper<User>
    /// {
    ///     public override void Configure(IDictionary<string, PropertyMapping> mappings)
    ///     {
    ///         base.Configure(mappings);
    /// 
    ///         // Id is of the column type "uniqueidentifier" in the DB and of "string" type for our property.
    ///         mappings["Id"].ColumnToPropertyAdapter = value => value.ToString();
    ///         mappings["Id"].PropertyToColumnAdapter = value => Guid.Parse(string)value);
    ///     }
    /// }
    /// ]]>
    /// </code>
    ///     <para>Look at the <see cref="Configure" /> documentation for more examples.</para>
    /// </example>
    public class EntityMapper<TEntity> : IEntityMapper<TEntity>
    {
        private readonly Func<TEntity> _factoryMethod;
        private readonly IDictionary<string, IPropertyMapping> _mappings = new Dictionary<string, IPropertyMapping>();
        private readonly IDictionary<string, IPropertyMapping> _keys = new Dictionary<string, IPropertyMapping>();
        private ICommandBuilder _builder = null;

        public EntityMapper(string tableName)
        {
            _factoryMethod = CreateInstanceFactory();
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Configure(_mappings);
            TableName = tableName;
        }

        object IEntityMapper.Create(IDataRecord record)
        {
            return Create(record);
        }

        /// <summary>
        ///     Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        void IEntityMapper.Map(IDataRecord source, object destination)
        {
            Map(source, (TEntity) destination);
        }

        /// <summary>
        ///     Free the mapping, no further changes may be made.
        /// </summary>
        /// <remarks>
        ///     <para>Called by the mapping provider when it's being added to it.</para>
        /// </remarks>
        public void Freeze()
        {
            _builder = CommandBuilderFactory.Create(this);
            foreach (var kvp in _mappings.Where(x => x.Value.IsPrimaryKey))
            {
                _keys.Add(kvp);
            }
        }


        /// <summary>
        ///     Gets table name
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        ///     All properties in this mapping
        /// </summary>
        public IDictionary<string, IPropertyMapping> Properties
        {
            get { return _mappings; }
        }

        /// <summary>
        /// Get the primary key 
        /// </summary>
        /// <param name="entity">entity to fetch key values from.</param>
        /// <returns>A single item in the array for a single PK column and one entry per column in composite primary key</returns>
        public KeyValuePair<string, object>[] GetKeys(object entity)
        {
            var values = new KeyValuePair<string, object>[_keys.Count];
            int index = 0;
            foreach (var kvp in _keys)
            {
                values[index++] = new KeyValuePair<string, object>(kvp.Key, kvp.Value.GetValue(entity));
            }
            return values;
        }

        /// <summary>
        ///     Used to create SQL commands which is specific for this entity.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The recommended approach for implementations is to retrieve the command builder from
        ///         <see cref="CommandBuilderFactory" /> when the <c>Freeze()</c> method is being invoked.
        ///         By doing so it's easy to adapt and precompile the command strings and logic before any invocations is made.
        ///     </para>
        /// </remarks>
        public ICommandBuilder CommandBuilder
        {
            get { return _builder; }
        }

        /// <summary>
        ///     Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        public void Map(IDataRecord source, TEntity destination)
        {
            string propertyName = null;
            try
            {
                foreach (var mapping in _mappings)
                {
                    propertyName = mapping.Key;
                    mapping.Value.Map(source, destination);
                }
            }
            catch (Exception exception)
            {
                throw new MappingException(typeof (TEntity),
                    string.Format("Failed to cast column value to property value for '{0}', property '{1}'.",
                        typeof (TEntity).FullName, propertyName), exception);
            }
        }


        /// <summary>
        ///     Used to map all properties which should be read from the database record.
        /// </summary>
        /// <param name="mappings">Dictionary which should be filled with all mappings</param>
        /// <remarks>
        ///     <para>
        ///         Will scan all properties and assign them a mapping, even if the setters are non-public. If no setter is
        ///         available
        ///         it will try to finding a field using the name convention where <c>FirstName</c> becomes <c>_firstName</c>.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <para>If you want to remove a property:</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings.Remove("CreatedAt");
        /// }
        /// ]]>
        /// </code>
        ///     <para>Example if the column type is <c>uniqueidentifier</c> and the property type is string:</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["Id"].ColumnToPropertyAdapter = value => value.ToString();
        /// }
        /// ]]>
        /// </code>
        ///     <para>If you have stored a child aggregate as a JSON string in a column</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["AuditLog"].ColumnToPropertyAdapter = value => JsonConvert.ToObject<IEnumerable<AuditEntry>>(value.ToString());
        /// }
        /// ]]>
        /// </code>
        ///     <para>To convert an int column in the db to an enum</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["State"].ColumnToPropertyAdapter = value => (UserState)value;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Configure(IDictionary<string, IPropertyMapping> mappings)
        {
            var type = typeof (TEntity);

            MapProperties(type);
        }

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
        public TEntity Create(IDataRecord record)
        {
            return _factoryMethod();
        }

        public static Func<TEntity> CreateInstanceFactory()
        {
            //credits: http://stackoverflow.com/questions/6582259/fast-creation-of-objects-instead-of-activator-createinstancetype

            var entityType = typeof (TEntity);
            var dynMethod = new DynamicMethod("Griffin$OBJ_FACTORY_" + entityType.Name, entityType, null, entityType);
            var ilGen = dynMethod.GetILGenerator();
            var constructor =
                entityType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                    Type.EmptyTypes, null);
            if (constructor == null)
                throw new MappingException(typeof (TEntity),
                    "Failed to find a default constructor for '" + typeof (TEntity).FullName + "'.");
            ilGen.Emit(OpCodes.Newobj, constructor);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TEntity>) dynMethod.CreateDelegate(typeof (Func<TEntity>));
        }

        private static Action<TEntity, object> CreateSetAccessor(FieldInfo field)
        {
            var setMethod = new DynamicMethod(field.Name, typeof (void), new[] {typeof (TEntity), typeof (object)});
            var generator = setMethod.GetILGenerator();
            var local = generator.DeclareLocal(field.DeclaringType);
            generator.Emit(OpCodes.Ldarg_0);
            if (field.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                generator.Emit(OpCodes.Stloc_0, local);
                generator.Emit(OpCodes.Ldloca_S, local);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, field.DeclaringType);
                generator.Emit(OpCodes.Stloc_0, local);
                generator.Emit(OpCodes.Ldloc_0, local);
            }
            generator.Emit(OpCodes.Ldarg_1);
            if (field.FieldType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, field.FieldType);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, field.FieldType);
            }
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            return (Action<TEntity, object>) setMethod.CreateDelegate(typeof (Action<TEntity, object>));
        }

        private void MapProperties(Type type)
        {
            var properties =
                typeof (TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var method = property.GetSetMethod(true);
                Action<TEntity, object> setter;
                if (method == null)
                {
                    var field = type.GetField("_" + char.ToLower(property.Name[0]) + property.Name.Substring(1),
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field == null)
                        continue;

                    var instance = Expression.Parameter(typeof (TEntity), "instance");
                    var value = Expression.Parameter(typeof (object), "value");
                    var result =
                        Expression.Lambda<Action<TEntity, object>>(
                            Expression.Assign(
                                Expression.Field(instance, field),
                                Expression.Convert(value, field.FieldType)),
                            instance,
                            value).Compile();

                    setter = result;
                }
                else
                {
                    var instance = Expression.Parameter(typeof (TEntity), "i");
                    var argument = Expression.Parameter(typeof (object), "a");
                    var setterCall = Expression.Call(
                        instance,
                        method,
                        Expression.Convert(argument, property.PropertyType));
                    var result = (Action<TEntity, object>) Expression.Lambda(setterCall, instance, argument)
                        .Compile();


                    //var m = method;
                    //setter = (instance, value) => m.Invoke(instance, new[] { value });
                    setter = result;
                }

                method = property.GetGetMethod(true);
                Func<TEntity, object> getter;
                if (method == null)
                {
                    var field = type.GetField("_" + char.ToLower(property.Name[0]) + property.Name.Substring(1),
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field == null)
                        continue;

                    //getter = field.GetValue;
                    var paramExpression = Expression.Parameter(typeof (TEntity), "instance");
                    Expression fieldGetter = Expression.Field(paramExpression, field.Name);
                    if (field.FieldType.IsPrimitive)
                        fieldGetter = Expression.Convert(fieldGetter, typeof (object));

                    var result =
                        Expression.Lambda<Func<TEntity, object>>(fieldGetter, paramExpression).Compile();
                    getter = result;
                }
                else
                {
                    //var m = method;
                    //getter = (instance) => m.Invoke(instance, null);

                    var instance = Expression.Parameter(typeof (TEntity), "i");
                    var prop = Expression.Property(instance, property);
                    var convert = Expression.TypeAs(prop, typeof (object));
                    var result = (Func<TEntity, object>) Expression.Lambda(convert, instance).Compile();
                    getter = result;
                }

                var mapping = new PropertyMapping<TEntity>(property.Name, setter, getter);
                _mappings.Add(property.Name, mapping);
            }
        }

        
    }
}