using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Asynchronous extensions for database connections
    /// </summary>
    public static class AsyncConnectionExtensions
    {
        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IDbConnection connection, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
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
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
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
        /// <param name="connection">connection to load entity from</param>
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
        public static Task<TEntity> FirstAsync<TEntity>(this IDbConnection connection, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
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
        /// <param name="connection">connection to load entity from</param>
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
        public static Task<TEntity> FirstAsync<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.FirstAsync(mapping);
            }
        }


        /// <summary>
        /// Insert an entity into the database
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to use</param>
        /// <param name="entity">Entity to insert.</param>
        /// <returns>Task to wait on for completion</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        public static async Task InsertAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to use</param>
        /// <param name="entity">Entity to update.</param>
        /// <returns>Task to wait on for completion</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        public static async Task UpdateAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">DB connection.</param>
        /// <param name="entity">Entity to remove.</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        public static async Task DeleteAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                mapper.CommandBuilder.DeleteCommand(cmd, entity);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">DB connection.</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Task to wait on for completion.</returns>
        /// <remarks>
        /// <para>Uses <see cref="EntityMappingProvider"/> to find the correct <c><![CDATA[ICrudEntityMapper<TEntity>]]></c></para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public async Task DeleteUser(int userId)
        /// {
        ///     await connection.DeleteAsync(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static async Task DeleteAsync<TEntity>(this IDbConnection connection, object constraints)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = string.Format("DELETE FROM {0} WHERE ", mapper.TableName);
                cmd.ApplyConstraints(mapper, constraints);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Execute a query directly
        /// </summary>
        /// <param name="connection">Connection to execute query on</param>
        /// <param name="sql">sql query</param>
        /// <param name="parameters">parameters used in the query</param>
        /// <returns>Task to wait on for completion</returns>
        /// <remarks>
        /// <para>Do note that the query must be using table column names and not class properties. No mapping is being made.</para>
        /// <para><c>null</c> is automatically replaced by <c>DBNull.Value</c> for the parameters</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public async Task Execute(IDbConnection connection)
        /// {
        ///     connection.ExecuteNonQueryAsync("UPDATE Users SET Discount = Discount + 10 WHERE OrganizationId = @orgId", new { orgId = 10});
        /// </code>
        /// </example>
        public static async Task ExecuteNonQueryAsync(this IDbConnection connection, string sql, object parameters = null)
        {
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var kvp in parameters.ToDictionary())
                    {
                        cmd.AddParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row (you must close the connection once done).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = await connection.ToEnumerable<User>("Age < 10");
        /// var users = await connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await connection.ToEnumerable<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await connection.ToEnumerable<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            return ToEnumerableAsync<TEntity>(connection, false, query, parameters);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
        /// <param name="query">Query or short query (<c>"id = @1"</c>)</param>
        /// <param name="parameters"></param>
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
        /// var users = await connection.ToEnumerable<User>(true, "Age < 10");
        /// var users = await connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = 37");
        /// var users = await connection.ToEnumerable<User>(true, "FirstName = @name", new { name = user.FirstName });
        /// var users = await connection.ToEnumerable<User>(true, "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection,
            bool ownsConnection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();

            var cmd = connection.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);
            var reader = await cmd.ExecuteReaderAsync();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "Age < 10");
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = 37");
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @name", new { name = user.FirstName });
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection,
            bool ownsConnection, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var cmd = connection.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);
            var reader = await cmd.ExecuteReaderAsync();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = await connection.ToListAsync<User>("Age < 10");
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await connection.ToListAsync<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await connection.ToListAsync<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            return await ToListAsync(connection, mapping, query, parameters);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = await connection.ToListAsync<User>("Age < 10");
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = await connection.ToListAsync<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = await connection.ToListAsync<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = await connection.ToListAsync<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbConnection connection, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (mapping == null) throw new ArgumentNullException("mapping");

            var cmd = connection.CreateDbCommand();
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