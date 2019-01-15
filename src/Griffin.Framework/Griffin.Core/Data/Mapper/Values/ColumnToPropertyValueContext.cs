using System;
using System.Data;

namespace Griffin.Data.Mapper.Values
{
    /// <summary>
    /// Used by <see cref="ColumnToPropertyValueHandler"/>.
    /// </summary>
    public class ColumnToPropertyValueContext
    {
        private readonly IDataRecord _record;
        private Type _entityType;

        public ColumnToPropertyValueContext(Type entityType, object value, IDataRecord record)
        {
            Value = value;
            _entityType = entityType;
            _record = record;
        }

        /// <summary>
        /// Column value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Entire record
        /// </summary>
        public IDataRecord Record
        {
            get
            {
                if (_record == null)
                    throw new MappingException(_entityType, "ConvertColumnValue2 is not supported for keys.");
                return _record;
            }
        }
    }
}