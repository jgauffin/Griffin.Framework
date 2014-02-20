using System.Data.Common;
using System.Threading.Tasks;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Extension methods for our AdoNet unit of work.
    /// </summary>
    public static class AsyncAdoNetUnitOfWorkExtensions
    {

        /// <summary>
        /// Insert a new row into the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="entity">entity to insert into the database.</param>
        /// <returns>Task to wait on for completion</returns>
        /// <remarks>
        /// <para>
        /// Will assign the PK value to the 
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var user = new User(10, "Jonas");
        /// using (var uow = UnitOfWorkFactory.Create())
        /// {
        ///     await uow.InsertAsync(user);
        /// }
        /// </code>
        /// </example>
        public static async Task InsertAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand)unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                //var keys = mapper.GetKeys(entity);
                //if (keys.Length == 1 && true)
                //{
                //    var id = await cmd.ExecuteScalarAsync();
                //    mapper.Properties[keys[0].Key].SetColumnValue(entity, id);
                //}
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="entity">Uses the primary key column(s), as defined in the mapping, to remove the entry.</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public async Task DeleteUser(int userId)
        /// {
        ///     return await _unitOfWork.DeleteAsync(new User { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static async Task DeleteAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand) unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.DeleteCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Find first row in db.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public async Task Ban(int userId)
        /// {
        ///     using (var uow = UnitOfWorkFactory.Create())
        ///     {
        ///         var user = await uow.FirstAsync(new { Id = 1 });
        ///         user.State = AccountState.Banned;
        ///         await uow.UpdateAsync(user);
        /// 
        ///         uow.SaveChanges();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<TEntity> FirstAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand)unitOfWork.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                cmd.ApplyConstraints(mapper, constraints);
                return await cmd.FirstAsync<TEntity>();
            }
        }


        /// <summary>
        /// DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="constraints">Constraints to be used. any field with '%' in the name will return in <c>LIKE</c> queries.</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public async Task DeleteUser(int userId)
        /// {
        ///     await _unitOfWork.DeleteAsync(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static async Task DeleteAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand) unitOfWork.CreateCommand())
            {
                cmd.CommandText = string.Format("DELETE FROM {0} WHERE ", mapper.TableName);
                cmd.ApplyConstraints(mapper, constraints);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Transaction wrapper</param>
        /// <param name="entity">Entity to update</param>
        /// <returns>Task to wait on for completion</returns>
        /// <example>
        /// <code>
        /// using (var uow = UnitOfWorkFactory.Create())
        /// {
        ///     var user = await uow.FirstAsync(new { Id = 1 });
        ///     user.State = AccountState.Banned;
        ///     await uow.UpdateAsync(user);
        /// 
        ///     uow.SaveChanges();
        /// }
        /// </code>
        /// </example>
        public static async Task UpdateAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();

            using (var cmd = (DbCommand)unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
