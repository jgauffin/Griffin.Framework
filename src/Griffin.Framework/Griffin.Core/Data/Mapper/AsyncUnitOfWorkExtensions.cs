using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Extension methods for our AdoNet unit of work.
    /// </summary>
    public static class AsyncAdoNetUnitOfWorkExtensions
    {
        /// <summary>
        ///     DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider" />
        ///     .
        /// </typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="entity">Uses the primary key column(s), as defined in the mapping, to remove the entry.</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <example>
        ///     <code>
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
        ///     DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider" />
        ///     .
        /// </typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="constraints">Constraints to be used. any field with '%' in the name will return in <c>LIKE</c> queries.</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <example>
        ///     <code>
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
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">UnitOfWork to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>
        ///     Entity if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     return await _connection.FirstOrDefaultAsync<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return await _connection.FirstOrDefaultAsync<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
                return cmd.FirstOrDefaultAsync<TEntity>();
            }
        }

        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">UnitOfWork to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>
        ///     Entity if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     return await _connection.FirstOrDefaultAsync<User>("WHERE age < @Age", new { Age = minAge });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.FirstOrDefaultAsync<TEntity>();
            }
        }

        /// <summary>
        /// Get an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">UnitOfWork to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Found entity</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     return await _connection.FirstAsync<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return await _connection.FirstAsync<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        /// <exception cref="EntityNotFoundException">Failed to find an entity mathing the query</exception>
        public static Task<TEntity> FirstAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
                return cmd.FirstAsync(mapping);
            }
        }

        /// <summary>
        /// Get an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">UnitOfWork to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>Found entity</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     return await _connection.FirstAsync<User>("WHERE id = @id", new { id = UserId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into:
        /// </para>
        /// <code>
        /// command.CommandText = "SELECT * FROM Users WHERE id = @id";
        /// var p = command.CreateParameter();
        /// p.Name = "id";
        /// p.Value = userId;
        /// command.Parameters.Add(p);
        /// </code>
        /// </example>
        /// <exception cref="EntityNotFoundException">Failed to find an entity mathing the query</exception>
        public static Task<TEntity> FirstAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, object parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.FirstAsync(mapping);
            }
        }

        /// <summary>
        ///     Insert a new row into the database.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider" />
        ///     .
        /// </typeparam>
        /// <param name="unitOfWork">Unit of work to execute command in.</param>
        /// <param name="entity">entity to insert into the database.</param>
        /// <returns>Task to wait on for completion</returns>
        /// <remarks>
        ///     <para>
        ///         Will assign the PK value to the
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// var user = new User(10, "Jonas");
        /// using (var uow = UnitOfWorkFactory.Create())
        /// {
        ///     await uow.InsertAsync(user);
        /// }
        /// </code>
        /// </example>
        public static async Task<object> InsertAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (EqualityComparer<TEntity>.Default.Equals(default(TEntity), entity)) throw new ArgumentNullException("entity");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand) unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                var keys = mapper.GetKeys(entity);
                if (keys.Length == 1)
                {
                    var id = await cmd.ExecuteScalarAsync();
                    if (id != null && id != DBNull.Value)
                        mapper.Properties[keys[0].Key].SetColumnValue(entity, id);
                    return id;
                }
                return await cmd.ExecuteScalarAsync();
            }
        }

        /// <summary>
        ///     Update an entity
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider" />
        ///     .
        /// </typeparam>
        /// <param name="unitOfWork">Transaction wrapper</param>
        /// <param name="entity">Entity to update</param>
        /// <returns>Task to wait on for completion</returns>
        /// <example>
        ///     <code>
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
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            var mapper = EntityMappingProvider.GetMapper<TEntity>();

            using (var cmd = (DbCommand) unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row (you must close the connection once done).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         For more information about the "query" and "parameters" arguments, see <see cref="CommandExtensions.ApplyQuerySql{TEntity}"/>.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // All these examples are valid:
        /// <![CDATA[
        /// var users = await unitOfWork.ToEnumerable<User>("Age < 10");
        /// var users = await unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await unitOfWork.ToEnumerable<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await unitOfWork.ToEnumerable<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            return ToEnumerableAsync<TEntity>(unitOfWork, false, query, parameters);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">UnitOfWork to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         For more information about the "query" and "parameters" arguments, see <see cref="CommandExtensions.ApplyQuerySql{TEntity}"/>.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // All these examples are valid:
        /// <![CDATA[
        /// var users = await unitOfWork.ToEnumerable<User>(true, "Age < 10");
        /// var users = await unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = 37");
        /// var users = await unitOfWork.ToEnumerable<User>(true, "FirstName = @name", new { name = user.FirstName });
        /// var users = await unitOfWork.ToEnumerable<User>(true, "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork,
            bool ownsConnection, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();

            var cmd = unitOfWork.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);
            var reader = await cmd.ExecuteReaderAsync();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
        /// <param name="mapping">Mapping used when translating table rows to .NET classes.</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         For more information about the "query" and "parameters" arguments, see <see cref="CommandExtensions.ApplyQuerySql{TEntity}"/>.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // All these examples are valid:
        /// <![CDATA[
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "Age < 10");
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = 37");
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @name", new { name = user.FirstName });
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork,
            bool ownsConnection, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var cmd = unitOfWork.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);
            var reader = await cmd.ExecuteReaderAsync();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         For more information about the "query" and "parameters" arguments, see <see cref="CommandExtensions.ApplyQuerySql{TEntity}"/>.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // All these examples are valid:
        /// <![CDATA[
        /// var users = await unitOfWork.ToListAsync<User>("Age < 10");
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await unitOfWork.ToListAsync<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await unitOfWork.ToListAsync<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<List<TEntity>> ToListAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            return await ToListAsync(unitOfWork, mapping, query, parameters);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="mapping">Mapping used to translate from db table rows to .NET object</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         For more information about the "query" and "parameters" arguments, see <see cref="CommandExtensions.ApplyQuerySql{TEntity}"/>.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // All these examples are valid:
        /// <![CDATA[
        /// var users = await unitOfWork.ToListAsync<User>("Age < 10");
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await unitOfWork.ToListAsync<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await unitOfWork.ToListAsync<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await unitOfWork.ToListAsync<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<List<TEntity>> ToListAsync<TEntity>(this IAdoNetUnitOfWork unitOfWork, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (mapping == null) throw new ArgumentNullException("mapping");
            if (query == null) throw new ArgumentNullException("query");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var cmd = unitOfWork.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);

            var items = new List<TEntity>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var entity = mapping.Create(reader);
                    mapping.Map(reader, entity);
                    items.Add((TEntity)entity);
                }
            }
            return items;
        }

    }
}