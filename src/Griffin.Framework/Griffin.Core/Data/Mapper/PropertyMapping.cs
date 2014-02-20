using System;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Used to convert the database column value and assign it to the property/field in the entity.
    /// </summary>
    public class PropertyMapping<TEntity> : IPropertyMapping
    {
        private readonly Action<TEntity, object> _setter;
        private readonly Func<TEntity, object> _getter;
        private ValueHandler _columnToPropertyAdapter;
        private ValueHandler _propertyToColumnAdapter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        /// <example>
        /// <code>
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
            PropertyToColumnAdapter = value => value;
            if (propertyName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                IsPrimaryKey = true;
        }

        /// <summary>
        /// Determines if this property can be written to
        /// </summary>
        public bool CanWrite { get { return _setter != null; }}

        /// <summary>
        /// Determines if this property can be read
        /// </summary>
        public bool CanRead { get { return _getter!= null; } }

        /// <summary>
        /// This property is a primary key
        /// </summary>
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
        public ValueHandler ColumnToPropertyAdapter
        {
            get { return _columnToPropertyAdapter; }
            set
            {
                if (value == null)
                    _columnToPropertyAdapter = x => x;
                else
                    _columnToPropertyAdapter = value;
            }
        }


        /// <summary>
        ///     Used to convert the property to the type used by the column.
        /// </summary>
        public ValueHandler PropertyToColumnAdapter
        {
            get { return _propertyToColumnAdapter; }
            set
            {
                if (value == null)
                    _propertyToColumnAdapter = x => x;
                else
                    _propertyToColumnAdapter = value;
            }
        }


        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        /// <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        public void Map(IDataRecord source, TEntity destination)
        {
            var value = source[ColumnName];
            if (value == DBNull.Value)
                return;

            var adapted = _columnToPropertyAdapter(value);
            _setter(destination, adapted);
        }

        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        /// <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        void IPropertyMapping.Map(IDataRecord source, object destination)
        {
            var value = source[ColumnName];
            if (value == DBNull.Value)
                return;

            var adapted = _columnToPropertyAdapter(value);
            _setter((TEntity)destination, adapted);
        }

        object IPropertyMapping.GetValue(object entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            var value= _getter((TEntity)entity);
            return _propertyToColumnAdapter(value);
        }

        /// <summary>
        /// Set property value by specifying a column value (i.e. use the <c>ColumnToPropertyAdapter</c> when assigning the value)
        /// </summary>
        /// <param name="entity">Entity to retrieve value from</param>
        /// <param name="value">Column value</param>
        /// <returns>Property value</returns>
        public void SetColumnValue(object entity, object value)
        {
            if (value == DBNull.Value)
                return;

            var adapted = _columnToPropertyAdapter(value);
            _setter((TEntity)entity, adapted);
        }
    }
}