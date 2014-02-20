using System;
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
        /// <param name="constraints">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>
        ///     Entity if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct mapper.</para>
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
            using (var cmd = (DbCommand)connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
                return cmd.FirstOrDefaultAsync<TEntity>();
            }
        }

        /// <summary>
        /// Get an object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">connection to load entity from</param>
        /// <param name="constraints">dynamic specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Found entity</returns>
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
            using (var cmd = (DbCommand)connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
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
        public static async Task InsertAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand)connection.CreateCommand())
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
        public static async Task UpdateAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand)connection.CreateCommand())
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
        public static async Task DeleteAsync<TEntity>(this IDbConnection connection, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = (DbCommand)connection.CreateCommand())
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
            using (var cmd = (DbCommand)connection.CreateCommand())
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
            using (var cmd = (DbCommand)connection.CreateCommand())
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


    }
}