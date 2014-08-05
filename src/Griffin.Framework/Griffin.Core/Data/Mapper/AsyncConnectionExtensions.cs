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
        /// <param name="query">SQL query, complete query or just <c>WHERE id = @id</c></param>
        /// <param name="constraints">Properties specified in query and their value. <c>new { id = userId }</c></param>
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
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IDbConnection connection, string query, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyQuerySql(mapping, query, constraints);
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
        /// <param name="query">SQL query, complete query or just <c>WHERE id = @id</c></param>
        /// <param name="constraints">Properties specified in query and their value. <c>new { id = userId }</c></param>
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
        public static Task<TEntity> FirstAsync<TEntity>(this IDbConnection connection, string query, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, constraints);
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
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The command is executed asynchronously.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>
        ///         As the returned item is a custom lazy loaded enumerable it's quite fast as nothing is mapped if you do like:
        ///     </para>
        ///     <example>
        ///         <code>
        /// <![CDATA[
        /// using (var cmd = connection.CreateCommand())
        /// {
        ///     cmd.CommandText = "SELECT * FROM Users";
        ///     var users = await cmd.ToEnumerable<User>();
        ///     return users.Skip(1000).Take(50).ToList();
        /// }
        /// ]]>
        /// </code>
        ///     </example>
        ///     <para>
        ///         Do note that it will still read all rows and is therefore slower than paging in the SQL server. It will however
        ///         use a lot less
        ///         allocations than building a complete list first.
        ///     </para>
        ///     <para>
        ///         If the result returnd from the query is all records that you want it's probably more effecient to use
        ///         <see cref="ToListAsync{TEntity}(System.Data.Common.DbCommand)" />.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        public static Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            return ToEnumerableAsync<TEntity>(connection, false);
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
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The command is executed asynchronously.
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
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection,
            bool ownsConnection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var cmd = connection.CreateDbCommand();
            var reader = await cmd.ExecuteReaderAsync();
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
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
        /// <param name="mapper">Mapper used to convert rows to entities</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The command is executed asynchronously.
        ///     </para>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>
        ///         Requires that a <c><![CDATA[IEntityMapper<TEntity>]]></c> is registered in the
        ///         <see cref="EntityMappingProvider" />.
        ///     </para>
        /// </remarks>
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbConnection connection,
            bool ownsConnection, IEntityMapper<TEntity> mapper)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var cmd = connection.CreateDbCommand();
            var reader = await cmd.ExecuteReaderAsync();
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         Uses the <see cref="EntityMappingProvider" /> to find the correct base mapper.
        ///     </para>
        ///     <para>
        ///         Make sure that you <c>await</c> the method, as nothing the reader is not disposed directly if you don't.
        ///     </para>
        /// </remarks>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return await ToListAsync(connection, mapping);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReaderAsync()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="mapper">Mapper to use when converting rows to entities</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         Make sure that you <c>await</c> the method, as nothing the reader is not disposed directly if you don't.
        ///     </para>
        /// </remarks>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbConnection connection,
            IEntityMapper<TEntity> mapper)
        {
            if (connection == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var cmd = connection.CreateDbCommand();
            var items = new List<TEntity>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var entity = mapper.Create(reader);
                    mapper.Map(reader, entity);
                    items.Add((TEntity)entity);
                }
            }
            return items;
        }


    }
}