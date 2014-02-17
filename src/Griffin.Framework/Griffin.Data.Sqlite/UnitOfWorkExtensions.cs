using System;
using System.Data;

namespace Griffin.Data.Sqlite
{
    public static class UnitOfWorkExtensions
    {
        public static bool TableExists(this IAdoNetUnitOfWork uow, string tableName)
        {
            using (var cmd = uow.CreateCommand())
            {
                cmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                cmd.AddParameter("tableName", tableName);
                return tableName.Equals(cmd.ExecuteScalar() as string, StringComparison.OrdinalIgnoreCase);
            }
        }

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