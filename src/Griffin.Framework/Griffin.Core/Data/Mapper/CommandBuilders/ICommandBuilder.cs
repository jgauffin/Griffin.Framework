using System.Data;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    /// Used to be able to adapt basic queries to the SQL dialetcs of each database engine.
    /// </summary>
    public interface ICommandBuilder
    {
        /// <summary>
        /// Gets prefix to use for data parameters (typically '@' or ':')
        /// </summary>
        char ParameterPrefix { get; }

        /// <summary>
        /// Modifies the command to execute an INSERT INTO using the entity as data
        /// </summary>
        /// <param name="command">Command that will be executed after this method call.</param>
        /// <param name="entity">Entity specified; If possible, set the entity primary key value when done.</param>
        /// <remarks>
        /// <para>The command should not be executed in the implementation of this interface. </para>
        /// <para>You may however execute commads </para>
        /// </remarks>
        void InsertCommand(IDbCommand command, object entity);

        /// <summary>
        /// Modifies the command to execute an UPDATE using the entity as data
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <param name="entity">Update is made on all properties but those specified as primary key (which is used in the WHERE clause)</param>
        void UpdateCommand(IDbCommand command, object entity);

        /// <summary>
        /// Modifies the command to execute a DELETE statement
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <param name="entity">Only primary key properties are used in the WHERE clause</param>
        void DeleteCommand(IDbCommand command, object entity);

        /// <summary>
        /// Truncate all rows in a table
        /// </summary>
        /// <param name="command">Command that will be executed after this method call</param>
        /// <remarks>
        /// <para>Some dialects have a special command which can be used to DELETE all rows from a table, everyone else should just use a DELETE statement without a WHERE clause.</para>
        /// </remarks>
        void TruncateCommand(IDbCommand command);

        /// <summary>
        /// Modify SQL statement so that the result is paged.
        /// </summary>
        /// <param name="command">Command to modify</param>
        /// <param name="pageNumber">One based index</param>
        /// <param name="pageSize">Items per page.</param>
        void Paging(IDbCommand command, int pageNumber, int pageSize);
    }
}