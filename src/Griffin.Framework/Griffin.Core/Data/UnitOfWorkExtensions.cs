using System;
using System.Data.Common;

namespace Griffin.Data
{
    /// <summary>
    ///     Expose <c>DbCommand</c> in the UoW.
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        ///     Create a Dbcommand
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         The async methods in ADO.NET is only exposed in the ADO.NET base classes (i.e. <c>DbCommand</c>) and not in the
        ///         interfaces. We'll therefore
        ///         have to violate Liskovs Substitution Principle to be able to access them. You should however be fine if you
        ///         seperate data from business and
        ///         just do integration tests for your data layer.
        ///     </para>
        /// </remarks>
        public static DbCommand CreateDbCommand(this IAdoNetUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var cmd = unitOfWork.CreateCommand() as DbCommand;
            if (cmd == null)
                throw new NotSupportedException(cmd.GetType().FullName +
                                                " do not inherit DbCommand. You can therefore not cast it to DbCommand to be able to use the async methods.");

            return cmd;
        }

        /// <summary>
        /// Execute a query directly
        /// </summary>
        /// <param name="unitOfWork">Unit of work to execute query in</param>
        /// <param name="sql">sql query</param>
        /// <param name="parameters">parameters used in the query</param>
        /// <remarks>
        /// <para>Do note that the query must be using table column names and not class properties. No mapping is being made.</para>
        /// <para><c>null</c> is automatically replaced by <c>DBNull.Value</c> for the parameters</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public void Execute(IDbConnection connection)
        /// {
        ///     uow.ExecuteNonQuery("UPDATE Users SET Discount = Discount + 10 WHERE OrganizationId = @orgId", new { orgId = 10});
        /// </code>
        /// </example>
        public static void ExecuteNonQuery(this IAdoNetUnitOfWork unitOfWork, string sql, object parameters = null)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var kvp in parameters.ToDictionary())
                    {
                        cmd.AddParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Execute a scalar query.
        /// </summary>
        /// <param name="unitOfWork">Unit of work to execute query in</param>
        /// <param name="sql">sql query</param>
        /// <param name="parameters">parameters used in the query</param>
        /// <remarks>
        /// <para>Do note that the query must be using table column names and not class properties. No mapping is being made.</para>
        /// <para><c>null</c> is automatically replaced by <c>DBNull.Value</c> for the parameters</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public void Execute(IDbConnection connection)
        /// {
        ///     uow.ExecuteScalar("UPDATE Users SET Discount = Discount + 10 WHERE OrganizationId = @orgId", new { orgId = 10});
        /// </code>
        /// </example>
        public static object ExecuteScalar(this IAdoNetUnitOfWork unitOfWork, string sql, object parameters = null)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var kvp in parameters.ToDictionary())
                    {
                        cmd.AddParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }
                return cmd.ExecuteScalar();
            }
        }

    }
}