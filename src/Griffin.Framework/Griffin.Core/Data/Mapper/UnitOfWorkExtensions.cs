using System;
using System.Collections.Generic;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Extension methods for the UnitOfWork.
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        ///     return unitOfWork.FirstOrDefault<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return unitOfWork.FirstOrDefault<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (constraints == null) throw new ArgumentNullException("constraints");

            var mapping = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
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
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        ///     return unitOfWork.FirstOrDefault<User>("WHERE age < @Age", new { Age = minAge });
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.FirstOrDefault<TEntity>();
            }
        }

        /// <summary>
        /// Get an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        ///     return unitOfWork.First<User>(new { Id = userId });
        /// }
        /// ]]>
        /// </code>
        /// <para>
        /// You can also use % for LIKE searches:
        /// </para>
        ///     <code>
        /// <![CDATA[
        /// return unitOfWork.First<User>(new { FirstName = 'Jon%', LastName = 'Gau%' });
        /// ]]>
        /// </code>
        /// <para>
        /// Which will translate into "WHERE FirstName LIKE 'Jon%' AND LastName LIKE 'Gau%'"
        /// </para>
        /// </example>
        /// <exception cref="EntityNotFoundException">Failed to find an entity mathing the query</exception>
        public static TEntity First<TEntity>(this IAdoNetUnitOfWork unitOfWork, object constraints)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var mapping = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
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
        /// <param name="unitOfWork">Unit of work to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        ///     return unitOfWork.First<User>("WHERE id = @id", new { id = UserId });
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
        public static TEntity First<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            using (var cmd = unitOfWork.CreateDbCommand())
            {
                cmd.ApplyQuerySql(mapping, query, parameters);
                return cmd.First(mapping);
            }
        }


        /// <summary>
        /// Truncate table (remove all rows without filling the transaction log)
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        public static void Truncate<TEntity>(this IAdoNetUnitOfWork unitOfWork)
        {
            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                mapper.CommandBuilder.TruncateCommand(cmd);
                cmd.ExecuteNonQuery();
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
        public static IList<TEntity> ToList<TEntity>(this IAdoNetUnitOfWork unitOfWork, object parameters)
        {
            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM {mapper.TableName} WHERE ";
                var args = parameters.ToDictionary();
                foreach (var parameter in args)
                {
                    cmd.AddParameter(parameter.Key, parameter.Value);
                    if (parameter.Value is string value && value.Contains("%"))
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
        /// <typeparam name="TEntity">Type of entity (must have a mapping registered in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="entity">The entity to create.</param>
        public static object Insert<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
            {
                try
                {
                    mapper.CommandBuilder.InsertCommand(cmd, entity);
                    var keys = mapper.GetKeys(entity);
                    if (keys.Length == 1)
                    {
                        var id = cmd.ExecuteScalar();
                        if (id != null && id != DBNull.Value)
                            mapper.Properties[keys[0].Key].SetProperty(entity, id);
                        return id;
                    }
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw cmd.CreateDataException(ex);
                }
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
            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
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
        /// Delete an existing entity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity (must have a mapping registred in the <see cref="EntityMappingProvider"/>)</typeparam>
        /// <param name="unitOfWork">Uow to extend</param>
        /// <param name="entity">The entity, must have the PK assigned.</param>
        public static void Delete<TEntity>(this IAdoNetUnitOfWork unitOfWork, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            using (var cmd = unitOfWork.CreateCommand())
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
        ///     Return an enumerable which uses lazy loading of each row (you must close the connection once done).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = unitOfWork.ToEnumerable<User>("Age < 10");
        /// var users = unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToEnumerable<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToEnumerable<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToEnumerable<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            return ToEnumerable<TEntity>(unitOfWork, false, query, parameters);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = unitOfWork.ToEnumerable<User>(true, "Age < 10");
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToEnumerable<User>(true, "FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToEnumerable<User>(true, "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IAdoNetUnitOfWork unitOfWork,
            bool ownsConnection, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            var cmd = unitOfWork.CreateDbCommand();
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
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
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
        /// var users = unitOfWork.ToEnumerable<User>(true, "Age < 10");
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToEnumerable<User>(true, "FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToEnumerable<User>(true, "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToEnumerable<User>(true, "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IAdoNetUnitOfWork unitOfWork, bool ownsConnection)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var mapping = EntityMappingProvider.GetCrudMapper<TEntity>();

            var cmd = unitOfWork.CreateDbCommand();
            try
            {
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
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "Age < 10");
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToEnumerable<User>(true, new CustomUserMapping(), "SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IAdoNetUnitOfWork unitOfWork,
            bool ownsConnection, IEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var cmd = unitOfWork.CreateDbCommand();
            cmd.ApplyQuerySql(mapping, query, parameters);
            var reader = cmd.ExecuteReader();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = unitOfWork.ToList<User>("Age < 10");
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToList<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToList<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IAdoNetUnitOfWork unitOfWork, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return ToList(unitOfWork, mapping, query, parameters);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="unitOfWork">Connection to invoke <c>ExecuteReader()</c> on (through a created <c>DbCommand</c>).</param>
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
        /// var users = unitOfWork.ToList<User>("Age < 10");
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = 37");
        /// var users = unitOfWork.ToList<User>("FirstName = @name", new { name = user.FirstName });
        /// var users = unitOfWork.ToList<User>("FirstName = @1 AND Age < @2", 'A%', 35);
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = @age LIMIT 1, 10", new { age = submittedAge });
        /// var users = unitOfWork.ToList<User>("SELECT * FROM Users WHERE Age = @1 LIMIT 1, 10", user.FirstName);
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IAdoNetUnitOfWork unitOfWork, IEntityMapper<TEntity> mapping, string query, params object[] parameters)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (mapping == null) throw new ArgumentNullException("mapping");

            var cmd = unitOfWork.CreateDbCommand();
            try
            {
                cmd.ApplyQuerySql(mapping, query, parameters);

                var items = new List<TEntity>();
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
