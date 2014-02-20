using System;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Synchronous connection extensions.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        ///     Fetches the first row from a query, but mapped as an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to create and execute our command on.</param>
        /// <param name="constraints">dynamic specifying the properties to use. All constraints are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Entity</returns>
        /// <exception cref="EntityNotFoundException">Failed to find entity</exception>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public User GetUser(string id)
        /// {
        ///     return _connection.First<User>(new { FirstName = "Jonas", LastName = "Gauffin" });
        /// }
        /// ]]>
        /// </code>
        /// <para>Alternative two (works exactly like the example above).</para>
        /// <code>
        /// <![CDATA[
        /// public User GetUser(User user)
        /// {
        ///     return _connection.First<User>(new { user.FirstName, user.LastName });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity First<TEntity>(this IDbConnection connection, dynamic constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                CommandExtensions.ApplyConstraints(cmd, mapper, constraints);
                return cmd.First<TEntity>();
            }
        }

        /// <summary>
        ///     Fetches the first row and maps it as an entity (if found).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to create and execute a command on.</param>
        /// <param name="constraints">dynamic specifying the properties to use. All constraints are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <returns>Entity if found; otherwise <c>null</c>.</returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public User GetUser(string id)
        /// {
        ///     return _connection.FirstOrDefault<User>(new { FirstName = "Jonas", LastName = "Gauffin" });
        /// }
        /// ]]>
        /// </code>
        /// <para>Alternative two (works exactly like the example above).</para>
        /// <code>
        /// <![CDATA[
        /// public User GetUser(User user)
        /// {
        ///     return _connection.FirstOrDefault<User>(new { user.FirstName, user.LastName });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IDbConnection connection, dynamic constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapper.TableName);
                CommandExtensions.ApplyConstraints(cmd, mapper, constraints);
                return cmd.FirstOrDefault<TEntity>();
            }
        }

        /// <summary>
        /// Truncate a table (remove all rows)
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to create and execute our command on</param>
        public static void Truncate<TEntity>(this IDbConnection connection)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.TruncateCommand(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Insert a new row into the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection create and execute our command on.</param>
        /// <param name="entity">entity to insert into the database.</param>
        /// <remarks>
        /// <para>
        /// Will assign the PK value to the 
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var user = new User(10, "Jonas");
        /// connection.Insert(user);
        /// </code>
        /// </example>
        public static void Insert<TEntity>(this IDbConnection connection, TEntity entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.InsertCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Transaction wrapper</param>
        /// <param name="entity">Entity to update</param>
        /// <returns>Task to wait on for completion</returns>
        /// <example>
        /// <code>
        /// var user = connection.First(new { Id = 1 });
        /// user.State = AccountState.Banned;
        /// connection.Update(user);
        /// </code>
        /// </example>
        public static void Update<TEntity>(this IDbConnection connection, TEntity entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.UpdateCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Unit of work to execute command in.</param>
        /// <param name="entity">Uses the primary key column(s), as defined in the mapping, to remove the entry.</param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void DeleteUser(int userId)
        /// {
        ///     connection.Delete(new User { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static void Delete<TEntity>(this IDbConnection connection, TEntity entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                mapper.CommandBuilder.DeleteCommand(cmd, entity);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// DELETE a row from the table.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Unit of work to execute command in.</param>
        /// <param name="constraints"><c>dynamic</c> specifying the properties to use. All parameters are joined with "AND" in the resulting SQL query. Any parameter with '%' in the value will be using LIKE instead of '='</param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void DeleteUser(int userId)
        /// {
        ///     connection.Delete(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>Alternative syntax:</para>
        /// <code>
        /// <![CDATA[
        /// public void DeleteUser(SomeDTO dto)
        /// {
        ///     connection.Delete(new { dto.Id });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static void Delete<TEntity>(this IDbConnection connection, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM " + mapper.TableName + " WHERE ";
                cmd.ApplyConstraints(mapper, constraints);
                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Execute a query directly
        /// </summary>
        /// <param name="connection">Connection to execute query on</param>
        /// <param name="sql">sql query</param>
        /// <param name="parameters">parameters used in the query</param>
        /// <remarks>
        /// <para>Do note that the query must be using table column names and not class properties. No mapping is being made.</para>
        /// <para><c>null</c> is automatically replaced by <c>DBNull.Value</c> for the parameters</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public void Execute(IDbConnection connection)
        /// {
        ///     connection.ExecuteNonQuery("UPDATE Users SET Discount = Discount + 10 WHERE OrganizationId = @orgId", new { orgId = 10});
        /// </code>
        /// </example>
        public static void ExecuteNonQuery(this IDbConnection connection, string sql, object parameters = null)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var kvp in parameters.ToDictionary())
                    {
                        cmd.AddParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }
                cmd.ExecuteNonQuery();
            }
        }
    }
}
