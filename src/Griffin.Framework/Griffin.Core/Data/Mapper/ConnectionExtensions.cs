using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper
{
    public static class ConnectionExtensions
    {
        public static TEntity First<TEntity>(this IDbConnection connection, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                var args = ObjectExtensions.ToDictionary(parameters);
                foreach (var parameter in args)
                {
                    Data.CommandExtensions.AddParameter(cmd, parameter.Key, parameter.Value);
                    cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " = @" + parameter.Key + " AND ";
                }
                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
                return cmd.First<TEntity>();
            }
        }

        public static TEntity FirstOrDefault<TEntity>(this IDbConnection connection, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                var args = ObjectExtensions.ToDictionary(parameters);
                foreach (var parameter in args)
                {
                    Data.CommandExtensions.AddParameter(cmd, parameter.Key, parameter.Value);
                    cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " = @" + parameter.Key + " AND ";
                }
                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
                return cmd.FirstOrDefault<TEntity>();
            }
        }

        public static IList<TEntity> ToList<TEntity>(this IDbConnection connection, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                var args = ObjectExtensions.ToDictionary(parameters);
                foreach (var parameter in args)
                {
                    Data.CommandExtensions.AddParameter(cmd, parameter.Key, parameter.Value);
                    if (parameter.Value.Contains("%"))
                        cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " LIKE @" + parameter.Key +
                                           " AND ";
                    else
                        cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " = @" + parameter.Key +
                                           " AND ";
                }
                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
                return cmd.ToList<TEntity>();
            }
        }


        public static void Truncate<TEntity>(this IDbConnection connection)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.TruncateCommand(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Insert<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.DeleteCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }


        public static void ExecuteNonQuery(this IDbConnection connection, string sql)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}