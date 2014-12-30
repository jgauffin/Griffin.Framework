using System.Data;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Sqlite
{
    /// <summary>
    /// Sqlite specific command builder.
    /// </summary>
    public class SqliteCommandBuilder : CommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteCommandBuilder"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public SqliteCommandBuilder(ICrudEntityMapper mapper) : base(mapper)
        {
        }

        /// <summary>
        /// Truncate all rows in a table
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <remarks>
        /// Will do a DELETE statement and reset the identity sequence generator.
        /// </remarks>
        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = string.Format("DELETE FROM {0};DELETE FROM SQLITE_SEQUENCE WHERE name='{0}';",
                TableName);
        }
    }
}