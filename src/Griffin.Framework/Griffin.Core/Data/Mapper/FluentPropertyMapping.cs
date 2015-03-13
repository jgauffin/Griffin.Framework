using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Used by <see cref="EntityMapper{TEntity}" />
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TPropertyType">Property type.</typeparam>
    public class FluentPropertyMapping<TEntity, TPropertyType>
    {
        private readonly PropertyMapping<TEntity> _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentPropertyMapping{TEntity, TPropertyType}" /> class.
        /// </summary>
        /// <param name="inner">Actual mapping object.</param>
        /// <exception cref="System.ArgumentNullException">inner</exception>
        public FluentPropertyMapping(PropertyMapping<TEntity> inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            _inner = inner;
        }

        /// <summary>
        /// Use if the column name differs from the property name
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>this</returns>
        /// <exception cref="System.ArgumentNullException">columnName</exception>
        public FluentPropertyMapping<TEntity, TPropertyType> ColumnName(string columnName)
        {
            if (columnName == null) throw new ArgumentNullException("columnName");
            _inner.ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// Do not update property with the column value.
        /// </summary>
        /// <returns>this</returns>
        public FluentPropertyMapping<TEntity, TPropertyType> NotForQueries()
        {
            _inner.NotForQueries();
            return this;
        }

        /// <summary>
        /// Do not write this property to the table.
        /// </summary>
        /// <returns>this</returns>
        public FluentPropertyMapping<TEntity, TPropertyType> NotForCrud()
        {
            _inner.NotForCrud();
            return this;
        }

        /// <summary>
        /// Do not map this propery.
        /// </summary>
        /// <returns></returns>
        public FluentPropertyMapping<TEntity, TPropertyType> Ignore()
        {
            _inner.NotForCrud();
            _inner.NotForQueries();
            return this;
        }

        /// <summary>
        /// Primary key (composite keys are supported).
        /// </summary>
        /// <returns>this</returns>
        public FluentPropertyMapping<TEntity, TPropertyType> PrimaryKey()
        {
            _inner.IsPrimaryKey = true;
            return this;
        }

        /// <summary>
        /// Primary key (composite keys are supported).
        /// </summary>
        /// <param name="isAutoIncremented">Specified as a auto incremented (identity) field. i.e. the DB generates the value, do not specify the field in INSERT statements.</param>
        /// <returns>this</returns>
        public FluentPropertyMapping<TEntity, TPropertyType> PrimaryKey(bool isAutoIncremented)
        {
            _inner.IsPrimaryKey = true;
            _inner.IsAutoIncrement = isAutoIncremented;
            return this;
        }

        /// <summary>
        /// Use if the column type differs from the property type.
        /// </summary>
        /// <param name="fromPropertyConverter">From property converter.</param>
        /// <returns>this</returns>
        /// <exception cref="System.ArgumentNullException">fromPropertyConverter</exception>
        /// <remarks>
        /// <para>
        /// Used in CRUD statements
        /// </para>
        /// </remarks>
        public FluentPropertyMapping<TEntity, TPropertyType> ToColumnValue(
            Func<TPropertyType, object> fromPropertyConverter)
        {
            if (fromPropertyConverter == null) throw new ArgumentNullException("fromPropertyConverter");
            _inner.PropertyToColumnAdapter = x => fromPropertyConverter((TPropertyType) x);
            return this;
        }

        /// <summary>
        /// Use if the property type differs from the column type.
        /// </summary>
        /// <param name="fromColumnConverter">From column converter.</param>
        /// <returns>this</returns>
        /// <exception cref="System.ArgumentNullException">fromColumnConverter</exception>
        /// <remarks>
        /// <para>
        /// Used in <code>SELECT</code> statements.
        /// </para>
        /// </remarks>
        public FluentPropertyMapping<TEntity, TPropertyType> ToPropertyValue(
            Func<object, TPropertyType> fromColumnConverter)
        {
            if (fromColumnConverter == null) throw new ArgumentNullException("fromColumnConverter");
            _inner.ColumnToPropertyAdapter = x => fromColumnConverter((TPropertyType) x);
            return this;
        }
    }
}