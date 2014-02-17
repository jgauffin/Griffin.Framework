using System.Data;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Sqlite
{
    public class SqliteCommandBuilder : CommandBuilder
    {
        public SqliteCommandBuilder(IEntityMapper mapper) : base(mapper)
        {
        }

        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = string.Format("DELETE FROM {0};DELETE FROM SQLITE_SEQUENCE WHERE name='{0}';",
                TableName);
        }
    }
}