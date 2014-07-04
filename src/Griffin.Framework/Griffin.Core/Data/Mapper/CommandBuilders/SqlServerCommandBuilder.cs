using System.Data;

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
        public SqlServerCommandBuilder(IEntityMapper mapper)
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
            command.CommandText += ";select @@IDENTITY";
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
    }
}
