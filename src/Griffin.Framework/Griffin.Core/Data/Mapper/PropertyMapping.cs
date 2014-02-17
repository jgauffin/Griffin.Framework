using System;
using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Used to convert the database column value and assign it to the property/field in the entity.
    /// </summary>
    public class PropertyMapping<TEntity> : IPropertyMapping
    {
        private readonly Func<TEntity, object> _getter;
        private readonly Action<TEntity, object> _setter;

        /// <summary>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        /// <example>
        ///     <code>
        /// var mapping = new PropertyMapping("Id", (instance, value) => ((User)instance).Id = (string)value);
        /// </code>
        /// </example>
        public PropertyMapping(string propertyName, Action<TEntity, object> setter, Func<TEntity, object> getter)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (setter == null) throw new ArgumentNullException("setter");
            if (getter == null) throw new ArgumentNullException("getter");

            _setter = setter;
            _getter = getter;
            PropertyName = propertyName;
            ColumnName = propertyName;
            ColumnToPropertyAdapter = value => value;
            if (propertyName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                IsPrimaryKey = true;
        }

        public bool CanWrite
        {
            get { return _setter != null; }
        }

        public bool CanRead
        {
            get { return _getter != null; }
        }

        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Name of the property in the entity
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        ///     Set if the column name is different from the property name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Used to convert the database value to the type used by the property
        /// </summary>
        public ValueHandler ColumnToPropertyAdapter { get; set; }


        /// <summary>
        ///     Used to convert the property to the type used by the column.
        /// </summary>
        public ValueHandler PropertyToColumnAdapter { get; set; }


        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        ///     <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        void IPropertyMapping.Map(IDataRecord source, object destination)
        {
            var value = source[ColumnName];
            if (value == DBNull.Value)
                return;

            var adapted = ColumnToPropertyAdapter(value);
            _setter((TEntity) destination, adapted);
        }

        object IPropertyMapping.GetValue(object entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return _getter((TEntity) entity);
        }

        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        ///     <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        public void Map(IDataRecord source, TEntity destination)
        {
            var value = source[ColumnName];
            if (value == DBNull.Value)
                return;

            var adapted = ColumnToPropertyAdapter(value);
            _setter(destination, adapted);
        }

        public object GetValue(TEntity entity)
        {
            if (EqualityComparer<TEntity>.Default.Equals(default(TEntity), entity))
                throw new ArgumentNullException("entity");

            return _getter(entity);
        }
    }


    /// <summary>
    /// </summary>
    /// <param name="originalValue">Value from column or property depending on the mapping direction</param>
    /// <returns></returns>
    public delegate object ValueHandler(object originalValue);
}