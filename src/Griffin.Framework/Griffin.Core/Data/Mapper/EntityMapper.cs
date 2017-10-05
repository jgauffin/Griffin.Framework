using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Griffin.Reflection;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Used to map a <see cref="IDataRecord" /> to an entity.
    /// </summary>
    /// <remarks>
    ///     <para>Just hides the non generic methods from the public contract.</para>
    /// </remarks>
    public class EntityMapper<TEntity> : IEntityMapper<TEntity>
    {
        private readonly Func<TEntity> _factoryMethod;
        private readonly IDictionary<string, IPropertyMapping> _mappings = new Dictionary<string, IPropertyMapping>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityMapper{TEntity}" /> class.
        /// </summary>
        public EntityMapper()
        {
            _factoryMethod = CreateInstanceFactory();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Configure(_mappings);
        }

        /// <summary>
        ///     All properties in this mapping
        /// </summary>
        public IDictionary<string, IPropertyMapping> Properties
        {
            get { return _mappings; }
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
        object IEntityMapper.Create(IDataRecord record)
        {
            return _factoryMethod();
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
                if (exception is MappingException)
                    throw;

                throw new MappingException(typeof (TEntity),
                    string.Format("Failed to cast column value to property value for '{0}.{1}'.",
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
        ///     Generate a delegate which can create the entity blasingly fast (compared to <c>Activator.CreateInstance()</c>).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Griffin.Data.Mapper.MappingException">
        ///     Failed to find a default constructor for ' + typeof
        ///     (TEntity).FullName + '.
        /// </exception>
        public static Func<TEntity> CreateInstanceFactory()
        {
            //credits: http://stackoverflow.com/questions/6582259/fast-creation-of-objects-instead-of-activator-createinstancetype

            var entityType = typeof (TEntity);
            var dynMethod = new DynamicMethod("Griffin$OBJ_FACTORY_" + entityType.Name, entityType, null, entityType);
            var ilGen = dynMethod.GetILGenerator();
            var constructor =
                entityType.GetTypeInfo().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                    CallingConventions.Standard, Type.EmptyTypes, null);
            if (constructor == null)
                throw new MappingException(typeof (TEntity),
                    "Failed to find a default constructor for '" + typeof (TEntity).FullName + "'.");
            ilGen.Emit(OpCodes.Newobj, constructor);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TEntity>) dynMethod.CreateDelegate(typeof (Func<TEntity>));
        }

        /// <summary>
        ///     Fluent syntax for defining a property mapping. Use in the constructor.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>mapping to configure.</returns>
        public FluentPropertyMapping<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var propertyInfo = propertyExpression.GetPropertyInfo();
            if (_mappings.ContainsKey(propertyInfo.Name))
                return new FluentPropertyMapping<TEntity, TProperty>((PropertyMapping<TEntity>)_mappings[propertyInfo.Name]);

            var setter = BuildSetter(typeof (TEntity), propertyInfo);
            var getter = BuildGetter(typeof (TEntity), propertyInfo);
            var mapping = new PropertyMapping<TEntity>(propertyInfo.Name, setter, getter)
            {
                PropertyType = propertyInfo.PropertyType
            };
            _mappings.Add(propertyInfo.Name, mapping);
            return new FluentPropertyMapping<TEntity, TProperty>(mapping);
        }

        private void MapProperties(Type type)
        {
            var properties = typeof (TEntity).GetTypeInfo().DeclaredProperties;
            foreach (var property in properties)
            {
                //already mapped in the constructor.
                if (_mappings.ContainsKey(property.Name))
                {
                    //remove mapping if not actions are allowed.
                    var mapping2 = _mappings[property.Name];
                    if (!mapping2.CanRead && !mapping2.CanWrite)
                        _mappings.Remove(property.Name);
                    
                    continue;
                }
                    

                var setter = BuildSetter(type, property);
                var getter = BuildGetter(type, property);

                var mapping = new PropertyMapping<TEntity>(property.Name, setter, getter);
                mapping.PropertyType = property.PropertyType;
                _mappings.Add(property.Name, mapping);
            }
        }

        private static Func<TEntity, object> BuildGetter(Type type, PropertyInfo property)
        {
            MethodInfo method;
            method = property.GetGetMethod(true);
            Func<TEntity, object> getter;
            if (method == null)
            {
                var field = type.GetField("_" + char.ToLower(property.Name[0]) + property.Name.Substring(1),
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                    return null;

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
            return getter;
        }

        private Action<TEntity, object> BuildSetter(Type type, PropertyInfo property)
        {
            var method = property.GetSetMethod(true);
            Action<TEntity, object> setter;
            if (method == null)
            {
                var field = type.GetField("_" + char.ToLower(property.Name[0]) + property.Name.Substring(1),
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                    return null;

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
            return setter;
        }
    }
}