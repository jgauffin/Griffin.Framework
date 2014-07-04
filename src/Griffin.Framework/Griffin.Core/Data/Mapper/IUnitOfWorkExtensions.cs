using System.Collections.Generic;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Extension methods for the UnitOfWork.
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// Truncate table (remove all rows without filling the transaction log)
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        public static void Truncate<TEntity>(this IAdoNetUnitOfWork unitOfWork)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.TruncateCommand(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Firsts the specified unit of work.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="parameters">Object with property names and values. We are using AND and equal to generate a WHERE clause.</param>
        /// <exception cref="EntityNotFoundException">Failed to find a matching entity</exception>
        /// <returns>Entity</returns>
        /// <example>
        /// <code>
        /// uow.First(new { FirstName = "Arne", LastName = "Svensson" });
        /// </code>
        /// </example>
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

        /// <summary>
        /// Find an entity (or return null)
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="parameters">Object with property names and values. We are using AND and equal to generate a WHERE clause.</param>
        /// <returns>Entity</returns>
        /// <example>
        /// <code>
        /// uow.First(new { FirstName = "Arne", LastName = "Svensson" });
        /// </code>
        /// </example>
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

        /// <summary>
        /// Find a collection of entities
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Collection (can be empty if no entries are found)</returns>
        /// <example>
        /// <code>
        /// // will generate a SQL clause: WHERE FirstName Like 'A%' AND LastName LIKE 'B%'
        /// uow.First(new { FirstName = "A%", LastName = "B%" });
        /// </code>
        /// </example>
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
                        cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " LIKE @" + parameter.Key + " AND ";
                    else
                        cmd.CommandText += mapper.Properties[parameter.Key].ColumnName + " = @" + parameter.Key + " AND ";
                }
                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
                return cmd.ToList<TEntity>();
            }
        }




        /// <summary>
        /// Insert a new item.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="entity">The entity to create.</param>
        public static void Insert<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="entity">The entity, must have the PK assigned.</param>
        public static void Update<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delete an existing entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="entity">The entity, must have the PK assigned.</param>
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
