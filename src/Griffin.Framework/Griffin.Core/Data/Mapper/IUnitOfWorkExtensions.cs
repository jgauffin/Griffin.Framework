using System.Collections.Generic;

namespace Griffin.Data.Mapper
{
    public static class UnitOfWorkExtensions
    {
        public static void Truncate<TEntity>(this IAdoNetUnitOfWork unitOfWork)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.TruncateCommand(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        public static TEntity First<TEntity>(this IAdoNetUnitOfWork unitOfWork, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT TOP 1 * FROM {0} WHERE ", mapper.TableName);
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

        public static TEntity FirstOrDefault<TEntity>(this IAdoNetUnitOfWork unitOfWork, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT TOP 1 * FROM {0} WHERE ", mapper.TableName);
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

        public static IList<TEntity> ToList<TEntity>(this IAdoNetUnitOfWork unitOfWork, dynamic parameters)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT TOP 100 * FROM {0} WHERE ", mapper.TableName);
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


        public static void Insert<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.DeleteCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }
    }
}