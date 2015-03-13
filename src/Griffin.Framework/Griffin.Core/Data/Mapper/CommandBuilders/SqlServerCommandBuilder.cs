using System.Data;
using System.Linq;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    /// Specializations for Sql Server.
    /// </summary>
    public class SqlServerCommandBuilder : CommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerCommandBuilder"/> class.
        /// </summary>
        /// <param name="mapper">Mapper that this builder can generate queries for.</param>
        public SqlServerCommandBuilder(ICrudEntityMapper mapper)
            : base(mapper)
        {
        }

        /// <summary>
        /// Generate an insert command, should end with a command that returns the insert identity.
        /// </summary>
        /// <param name="command">Command to add the query to</param>
        /// <param name="entity">Entity to store</param>
        /// <remarks>
        /// Last statement will return @@identity. 
        /// </remarks>
        public override void InsertCommand(IDbCommand command, object entity)
        {
            base.InsertCommand(command, entity);

            var keys = Mapper.GetKeys(entity);
            if (keys.Count() != 1)
                return;

            var key = Mapper.Properties.First(x => x.Key == keys.First().Key);
            if (!key.Value.IsAutoIncrement)
                return;

            command.CommandText += ";select SCOPE_IDENTITY()";
        }

        /// <summary>
        /// Uses TRUNCATE TABLE
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <remarks>
        /// Will do a TRUNCATE TABLE statement
        /// </remarks>
        public override void TruncateCommand(IDbCommand command)
        {
            command.CommandText = "TRUNCATE TABLE " + TableName;
        }

        /// <summary>
        /// Uses the SQL Server 2012 syntax (OFFSET/FETCH NEXT)
        /// </summary>
        /// <param name="command">command to modify</param>
        /// <param name="pageNumber">One based index</param>
        /// <param name="pageSize">Items per page.</param>
        public override void Paging(IDbCommand command, int pageNumber, int pageSize)
        {
            command.CommandText += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY",
                (pageNumber - 1) * pageSize, pageSize);
        }
    }
}
