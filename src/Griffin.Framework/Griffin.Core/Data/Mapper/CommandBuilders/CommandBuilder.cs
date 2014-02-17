using System;
using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    ///     Base class for command builders
    /// </summary>
    /// <remarks>
    ///     Creates SQL commands per the SQL92 standard. Inherit this class to customize different commands.
    /// </remarks>
    public class CommandBuilder : ICommandBuilder
    {
        private readonly List<IPropertyMapping> _keys = new List<IPropertyMapping>();
        private readonly IEntityMapper _mapper;
        private readonly string _tableName;
        private readonly List<IPropertyMapping> _values = new List<IPropertyMapping>();

        public CommandBuilder(IEntityMapper mapper)
        {
            if (mapper == null) throw new ArgumentNullException("mapper");

            _mapper = mapper;
            _tableName = mapper.TableName;
            foreach (var property in mapper.Properties.Values)
            {
                if (property.IsPrimaryKey)
                    _keys.Add(property);
                else
                    _values.Add(property);
            }
        }

        public string TableName
        {
            get { return _tableName; }
        }


        public virtual void InsertCommand(IDbCommand command, object entity)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (entity == null) throw new ArgumentNullException("entity");

            var columns = "";
            var values = "";
            foreach (var key in _keys)
            {
                var value = key.GetValue(entity);
                if (value == null)
                    continue;
                columns += string.Format("{0}, ", key.ColumnName);
                values += string.Format("@{0}, ", key.PropertyName);
                command.AddParameter(key.PropertyName, value);
            }
            foreach (var prop in _values)
            {
                var value = prop.GetValue(entity);
                columns += string.Format("{0}, ", prop.ColumnName);
                values += string.Format("@{0}, ", prop.PropertyName);
                command.AddParameter(prop.PropertyName, value ?? DBNull.Value);
            }
            if (command.Parameters.Count == 0)
                throw new DataException("No values were added to the query for " + entity);

            command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES({2})",
                TableName,
                columns.Remove(columns.Length - 2, 2),
                values.Remove(values.Length - 2, 2));
        }

        public void UpdateCommand(IDbCommand command, object entity)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (entity == null) throw new ArgumentNullException("entity");

            var updates = "";
            var where = "";
            foreach (var property in _values)
            {
                var value = property.GetValue(entity);
                updates += string.Format("{0}=@{1}, ", property.ColumnName, property.PropertyName);
                command.AddParameter(property.PropertyName, value);
            }
            if (command.Parameters.Count == 0)
                throw new DataException("At least one property (other than primary keys) must be specified.");

            foreach (var property in _keys)
            {
                var value = property.GetValue(entity);
                if (value == null || value == DBNull.Value)
                    throw new DataException(
                        string.Format("Entity {0}' do not contain a value for the key property '{1}'", entity,
                            property.PropertyName));
                where += property.ColumnName + "=" + "@" + property.PropertyName + " AND ";
                command.AddParameter(property.PropertyName, value);
            }

            command.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}",
                TableName,
                updates.Remove(updates.Length - 2, 2),
                @where.Remove(@where.Length - 5, 5));
        }

        public void DeleteCommand(IDbCommand command, object entity)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (entity == null) throw new ArgumentNullException("entity");

            var where = "";
            foreach (var property in _keys)
            {
                var value = property.GetValue(entity);
                if (value == null || value == DBNull.Value)
                    throw new DataException(
                        string.Format("Entity {0}' do not contain a value for the key property '{1}'", entity,
                            property.PropertyName));

                where += string.Format("{0}=" + "@{1} AND ", property.ColumnName, property.PropertyName);
                command.AddParameter(property.PropertyName, value);
            }

            command.CommandText = string.Format("DELETE FROM {0} WHERE {1}",
                TableName,
                @where.Remove(@where.Length - 5, 5));
        }

        public virtual void TruncateCommand(IDbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            command.CommandText = string.Format("DELETE FROM {0}", TableName);
        }
    }
}