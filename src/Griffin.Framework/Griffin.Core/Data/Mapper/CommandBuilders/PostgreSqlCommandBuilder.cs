using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    /// Will fetch the sequence value during inserts, truncate table (including the sequence) and page accordingly.
    /// </summary>
    public class PostgreSqlCommandBuilder : CommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBuilder"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <exception cref="System.ArgumentNullException">mapper</exception>
        public PostgreSqlCommandBuilder(ICrudEntityMapper mapper)
            : base(mapper)
        {
        }

        /// <summary>
        /// Generate an insert command, should end with a command that returns the insert identity.
        /// </summary>
        /// <param name="command">Command to add the query to</param>
        /// <param name="entity">Entity to store</param>
        /// <exception cref="System.ArgumentNullException">
        /// command
        /// or
        /// entity
        /// </exception>
        /// <exception cref="System.Data.DataException">No values were added to the query for  + entity</exception>
        public override void InsertCommand(IDbCommand command, object entity)
        {
            base.InsertCommand(command, entity);

            var keys = Mapper.GetKeys(entity);
            if (keys.Count() != 1)
                return;

            var key = Mapper.Properties.First(x => x.Key == keys.First().Key);
            if (!key.Value.IsAutoIncrement)
                return;

            var name = TableName + "_" + key.Key;
            command.CommandText += command + ";select currval('" + name + "_seq')";
        }

        /// <summary>
        /// Truncate all rows in a table
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <exception cref="System.ArgumentNullException">command</exception>
        /// <remarks>
        /// Will do a DELETE statement
        /// </remarks>
        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = "TRUNCATE TABLE " + TableName + " RESTART IDENTITY";
        }

        /// <summary>
        /// Uses LIMIT/OFFSET
        /// </summary>
        /// <param name="command">Command to modify</param>
        /// <param name="pageNumber">One based index</param>
        /// <param name="pageSize">Items per page.</param>
        public override void Paging(IDbCommand command, int pageNumber, int pageSize)
        {
            if (pageNumber <= 1)
            {
                command.CommandText += " LIMIT " + pageSize;
            }
            else
            {
                command.CommandText += string.Format(" LIMIT {0} OFFSET {1}", pageSize, ((pageNumber - 1)*pageSize));
            }
        }
    }
}
