using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Synchronous connection extensions.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// public User GetUser(int userId)
        /// {
        ///     return _connection.FirstOrDefault<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return _connection.FirstOrDefault<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IDbConnection connection, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
                return cmd.FirstOrDefault<TEntity>();
            }
        }

        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// public User GetUser(int userId)
        /// {
        ///     return _connection.FirstOrDefault<User>("WHERE age < @Age", new { Age = minAge });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.FirstOrDefault<TEntity>();
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
        /// public User GetUser(int userId)
        /// {
        ///     return _connection.First<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return _connection.First<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        /// <exception cref="EntityNotFoundException">Failed to find an entity mathing the query</exception>
        public static TEntity First<TEntity>(this IDbConnection connection, object constraints)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ", mapping.TableName);
                cmd.ApplyConstraints(mapping, constraints);
                return cmd.First(mapping);
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
        /// public User GetUser(int userId)
        /// {
        ///     return _connection.First<User>("WHERE id = @id", new { id = UserId });
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
        public static TEntity First<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.First(mapping);
            }
        }


        /// <summary>
        /// Cast <c>IDbCommand</c> to <c>DbCommand</c> to be able to access the async methods.
        /// </summary>
        /// <param name="connection">Connection used as a factory</param>
        /// <returns>Command</returns>
        /// <exception cref="NotSupportedException">The created command cannot be cast to DbCommand.</exception>
        public static DbCommand CreateDbCommand(this IDbConnection connection)
        {
            var cmd = connection.CreateCommand() as DbCommand;
            if (cmd == null)
                throw new NotSupportedException("Failed to cast connection.CreateCommand() to DbCommand, connection type: " + connection.GetType().FullName + ". We need to be able to cast IDbCommand to DbCommand to gain access to the async ADO.NET methods.");

            return cmd;
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
                try
                {
                    mapper.CommandBuilder.TruncateCommand(cmd);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
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
        public static object Insert<TEntity>(this IDbConnection connection, TEntity entity)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            using (var cmd = connection.CreateCommand())
            {
                try
                {
                    mapper.CommandBuilder.InsertCommand(cmd, entity);
                    var keys = mapper.GetKeys(entity);
                    if (keys.Length == 1)
                    {
                        var id = cmd.ExecuteScalar();
                        if (id != null && id != DBNull.Value)
                            mapper.Properties[keys[0].Key].SetColumnValue(entity, id);
                        return id;
                    }
                    return cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
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
                try
                {
                    mapper.CommandBuilder.UpdateCommand(cmd, entity);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
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
                try
                {
                    mapper.CommandBuilder.DeleteCommand(cmd, entity);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
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
                try
                {
                    cmd.CommandText = "DELETE FROM " + mapper.TableName + " WHERE ";
                    cmd.ApplyConstraints(mapper, constraints);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
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
                try
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
                catch (Exception e)
                {
                    throw cmd.CreateDataException(e);
                }
            }
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row (you must close the connection once done).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = connection.ToEnumerable<User>("Age < 10");
        /// var users = connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToEnumerable<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToEnumerable<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = connection.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            return ToEnumerable<TEntity>(connection, false, query, parameters);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = connection.ToEnumerable<User>(true, "Age < 10");
        /// var users = connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToEnumerable<User>(true, "FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToEnumerable<User>(true, "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = connection.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbConnection connection,
            bool ownsConnection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();

            var cmd = connection.CreateDbCommand();
            try
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                var reader = cmd.ExecuteReader();
                return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "Age < 10");
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = connection.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbConnection connection,
            bool ownsConnection, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var cmd = connection.CreateDbCommand();
            try
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                var reader = cmd.ExecuteReader();
                return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>A list.</returns>
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
        /// var users = connection.ToList<User>("Age < 10");
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToList<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToList<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IDbConnection connection, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            return ToList(connection, mapping, query, parameters);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="mapping">Mapping used to translate from db table rows to .NET object</param>
        /// <param name="query">Query or short query (<c><![CDATA["projectId = @id AND dateCreated < @minDate"]]></c>)</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>A list.</returns>
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
        /// var users = connection.ToList<User>("Age < 10");
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToList<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToList<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IDbConnection connection, ICrudEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (mapping == null) throw new ArgumentNullException("mapping");

            var cmd = connection.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);

            var items = new List<TEntity>();
            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entity = mapping.Create(reader);
                        mapping.Map(reader, entity);
                        items.Add((TEntity)entity);
                    }
                }
                return items;
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="connection">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="mapping">Mapping used to translate from db table rows to .NET object</param>
        /// <param name="query">Query</param>
        /// <param name="parameters">Anonymous object (<c>new { id = dto.ProjectId, @minDate = dto.MinDate }</c>), a dictionary or a value array</param>
        /// <returns>A list.</returns>
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
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE FirstName = @name", new { name = user.FirstName });
        /// var users = connection.ToList<User>("SELECT * FROM Users WHERE FirstName = @1 AND Age < @2", 'A%', 35);
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IDbConnection connection, IEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (mapping == null) throw new ArgumentNullException("mapping");

            var cmd = connection.CreateDbCommand();
            cmd.ApplyQuerySql<TEntity>(mapping, query, parameters);

            var items = new List<TEntity>();
            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entity = mapping.Create(reader);
                        mapping.Map(reader, entity);
                        items.Add((TEntity)entity);
                    }
                }
                return items;
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }
    }
}
