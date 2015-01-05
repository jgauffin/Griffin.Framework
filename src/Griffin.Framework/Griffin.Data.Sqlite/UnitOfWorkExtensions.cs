using System;
using System.Data;

namespace Griffin.Data.Sqlite
{
    /// <summary>
    /// Sqlite specific methods for a unit of work
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// Check if a table exists
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>
        ///   <c>true</c> if the table exist; otherwise <c>false</c>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">tableName</exception>
        public static bool TableExists(this IAdoNetUnitOfWork uow, string tableName)
        {
            if (tableName == null) throw new ArgumentNullException("tableName");
            using (var cmd = uow.CreateCommand())
            {
                cmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                cmd.AddParameter("tableName", tableName);
                return tableName.Equals(cmd.ExecuteScalar() as string, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Check if a table exists
        /// </summary>
        /// <param name="connection">The db connection.</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>
        ///   <c>true</c> if the table exist; otherwise <c>false</c>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">tableName</exception>
        public static bool TableExists(this IDbConnection connection, string tableName)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                cmd.AddParameter("tableName", tableName);
                return tableName.Equals(cmd.ExecuteScalar() as string, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}