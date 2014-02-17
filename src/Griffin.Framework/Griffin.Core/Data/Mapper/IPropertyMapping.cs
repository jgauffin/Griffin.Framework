using System.Data;

namespace Griffin.Data.Mapper
{
    public interface IPropertyMapping
    {
        bool CanWrite { get; }
        bool CanRead { get; }
        bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Used to convert the database value to the type used by the property
        /// </summary>
        ValueHandler ColumnToPropertyAdapter { get; set; }


        /// <summary>
        ///     Used to convert the property to the type used by the column.
        /// </summary>
        ValueHandler PropertyToColumnAdapter { get; set; }


        /// <summary>
        ///     Name of the property in the entity
        /// </summary>
        string PropertyName { get; set; }

        /// <summary>
        ///     Set if the column name is different from the property name
        /// </summary>
        string ColumnName { get; set; }

        /// <summary>
        ///     Convert the value in the specified record and assign it to the property in the specified instance
        /// </summary>
        /// <param name="source">Database record</param>
        /// <param name="destination">Entity instance</param>
        /// <remarks>
        ///     <para>Will exit the method without any assignment if the value is <c>DBNull.Value</c>.</para>
        /// </remarks>
        void Map(IDataRecord source, object destination);

        object GetValue(object entity);
    }
}