using System.Data;

namespace Griffin.Data
{
    /// <summary>
    ///     ADO.NET features for a Unit Of Work
    /// </summary>
    public interface IAdoNetUnitOfWork : IUnitOfWork
    {
        /// <summary>
        ///     Create a new command
        /// </summary>
        /// <returns>Created command (never <c>null</c>)</returns>
        /// <remarks>
        ///     <para>The created command have been enlisted in the local transaction which is wrapped by this Unit Of Work.</para>
        /// </remarks>
        /// <exception cref="DataException">Failed to create the command</exception>
        IDbCommand CreateCommand();

        /// <summary>
        /// Execute a SQL query within the transaction
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void Execute(string sql, object parameters);
    }
}