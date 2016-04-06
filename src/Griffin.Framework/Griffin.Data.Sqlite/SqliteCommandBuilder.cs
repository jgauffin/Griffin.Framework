using System.Data;
using System.Linq;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Sqlite
{
    /// <summary>
    ///     Sqlite specific command builder.
    /// </summary>
    public class SqliteCommandBuilder : CommandBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteCommandBuilder" /> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public SqliteCommandBuilder(ICrudEntityMapper mapper) : base(mapper)
        {
        }

        /// <summary>
        ///     Generate an insert command, should end with a command that returns the insert identity.
        /// </summary>
        /// <param name="command">Command to add the query to</param>
        /// <param name="entity">Entity to store</param>
        /// <remarks>
        ///     Last statement will return @@identity.
        /// </remarks>
        public override void InsertCommand(IDbCommand command, object entity)
        {
            base.InsertCommand(command, entity);

            var gotAutoIncrement = Mapper.Properties.Any(x => x.Value.IsAutoIncrement && x.Value.IsPrimaryKey);
            if (!gotAutoIncrement)
                return;

            command.CommandText += ";select last_insert_rowid()";
        }

        /// <summary>
        ///     Uses the SQLite - LIMIT [pageSize] OFFSET [pageNumber]
        ///     LIMIT [no of rows] OFFSET [row num]
        /// </summary>
        /// <param name="command">command to modify</param>
        /// <param name="pageNumber">One based index</param>
        /// <param name="pageSize">Items per page.</param>
        public override void Paging(IDbCommand command, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1)*pageSize;
            command.CommandText += string.Format(" LIMIT {1} OFFSET {0}",
                offset, pageSize);
        }

        /// <summary>
        ///     Truncate all rows in a table
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <remarks>
        ///     Will do a DELETE statement and reset the identity sequence generator.
        /// </remarks>
        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = string.Format("DELETE FROM {0};DELETE FROM SQLITE_SEQUENCE WHERE name='{0}';",
                TableName);
        }
    }
}