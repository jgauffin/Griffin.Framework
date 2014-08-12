using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    ///     <para>
    ///         This mapper is konventional based. If there is a column named <c>"Id"</c> this mapper will assume that that is
    ///         the primary key. If you do not have
    ///         an <c>"Id"</c> id column you need to inherit this class and overide the <c>Configure</c> method:
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : CrudEntityMapper<User>
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
    ///         All mappers must have a parameterless constructor, but you can set it as non-public if you do not want to
    ///         expose
    ///         it.
    ///     </para>
    ///     <para>
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         You can just create an empty class like below if there is an one-one mapping between the table and your entity
    ///         class. It will
    ///         automatically be
    ///         picked up by the <see cref="AssemblyScanningMappingProvider" />.
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : CrudEntityMapper<User>
    /// {
    /// }
    /// ]]>
    /// </code>
    ///     <para>You can also customize the mappings</para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : CrudEntityMapper<User>
    /// {
    ///     public override void Configure(IDictionary<string, PropertyMapping> mappings)
    ///     {
    ///         base.Configure(mappings);
    /// 
    ///         // Id is per default set to primary key, but any other name must be configured
    ///         // you can set multiple properties as a key too (composite key)
    ///         mappings["Id"].IsPrimaryKey = true;
    /// 
    ///         // UserId is of the column type "uniqueidentifier" in the DB and of "string" type for our property.
    ///         mappings["UserId"].ColumnToPropertyAdapter = value => value.ToString();
    ///         mappings["UserId"].PropertyToColumnAdapter = value => Guid.Parse(string)value);
    ///     }
    /// }
    /// ]]>
    /// </code>
    ///     <para>Look at the <see cref="EntityMapper{TEntity}.Configure" /> documentation for more examples.</para>
    /// </example>
    public class CrudEntityMapper<TEntity> : EntityMapper<TEntity>, ICrudEntityMapper<TEntity>
    {
        private readonly IDictionary<string, IPropertyMapping> _keys = new Dictionary<string, IPropertyMapping>();
        private ICommandBuilder _builder = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CrudEntityMapper{TEntity}" /> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public CrudEntityMapper(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException("tableName");
            TableName = tableName;
        }

        /// <summary>
        ///     Gets table name
        /// </summary>
        public virtual string TableName { get; set; }


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
        ///     <para>Called by the mapping provider when the mapping have been added to it.</para>
        ///     <para>
        ///         The purpose is to allow the mapping implementations to do post process once the mappings have been fully
        ///         configured. 
        ///     </para>
        /// </remarks>
        public void Freeze()
        {
            _builder = CommandBuilderFactory.Create(this);
            foreach (var kvp in Properties.Where(x => x.Value.IsPrimaryKey))
            {
                _keys.Add(kvp);
            }

            if (_keys.Count != 0)
                return;

            if (Properties.ContainsKey("Id"))
                _keys.Add("Id", Properties["Id"]);
        }

        /// <summary>
        ///     Get the primary key
        /// </summary>
        /// <param name="entity">entity to fetch key values from.</param>
        /// <returns>A single item in the array for a single PK column and one entry per column in composite primary key</returns>
        public KeyValuePair<string, object>[] GetKeys(object entity)
        {
            var values = new KeyValuePair<string, object>[_keys.Count];
            var index = 0;
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
    }
}