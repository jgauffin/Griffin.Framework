using System;
using System.Data;
using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.CommandBuilders
{
    public class FakePropertyMapping : IPropertyMapping
    {
        public FakePropertyMapping(string propertyName, string columnName)
        {
            PropertyName = propertyName;
            ColumnName = columnName;
        }

        public bool CanWrite { get; set; }
        public bool CanRead { get; set; }
        public bool IsPrimaryKey { get; set; }
        public object Value { get; set; }

        /// <summary>
        ///     Used to convert the database value to the type used by the property
        /// </summary>
        public ValueHandler ColumnToPropertyAdapter { get; set; }

        /// <summary>
        ///     Used to convert the property to the type used by the column.
        /// </summary>
        public ValueHandler PropertyToColumnAdapter { get; set; }

        /// <summary>
        ///     Name of the property in the entity
        /// </summary>
        public string PropertyName { get; set; }

        public string TableName { get; set; }

        /// <summary>
        ///     Set if the column name is different from the property name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        /// <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        public void Map(IDataRecord source, object destination)
        {
            
        }

        public object GetValue(object entity)
        {
            return Value;
        }
    }
}