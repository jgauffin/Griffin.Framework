using System.Data;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    /// Specializations for Sql Server.
    /// </summary>
    public class SqlServerCommandBuilder : CommandBuilder
    {
        public SqlServerCommandBuilder(IEntityMapper mapper)
            : base(mapper)
        {
        }

        public override void InsertCommand(IDbCommand command, object entity)
        {
            base.InsertCommand(command, entity);
            command.CommandText += ";select @@IDENTITY";
        }

        /// <summary>
        /// Uses TRUNCATE TABLE
        /// </summary>
        /// <param name="command"></param>
        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = "TRUNCATE TABLE " + TableName;
        }
    }
}
