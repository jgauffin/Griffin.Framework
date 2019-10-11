using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Extensions for <see cref="IDbCommand" />.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Takes an anonymous/dynamic objects and convert it into a WHERE clause using the supplied mapping.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to add parameters to (should end with " WHERE " so that this method can add the constraints properly)</param>
        /// <param name="mapper">Mapper to use to convert properties to columns</param>
        /// <param name="constraints">properties in an anonymous object</param>
        internal static void ApplyConstraints<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper, object constraints)
        {
            if (constraints == null)
                return;

            var crudMapper = mapper as ICrudEntityMapper<TEntity>;
            var dictionary = constraints.ToDictionary();
            if (dictionary.Count == 0 && (constraints.GetType().IsPrimitive || constraints is string) && crudMapper != null)
            {
                var keys = crudMapper.Properties.Where(x => x.Value.IsPrimaryKey).ToList();
                if (keys.Count != 1)
                {
                    throw new MappingException(typeof(TEntity), "Expected to find ONE primary key column.");
                }

                cmd.AddParameter(keys[0].Value.ColumnName, constraints);
                return;
            }

            var whereClause = "";
            foreach (var kvp in dictionary)
            {
                var columnName = kvp.Key;
                var propertyName = kvp.Key;
                var prefix = crudMapper?.CommandBuilder.ParameterPrefix ?? '@';
                var value = kvp.Value;

                if (crudMapper != null)
                {
                    if (!crudMapper.Properties.TryGetValue(kvp.Key, out var propertyMapping))
                    {
                        // we just assume that the user specified the column name directly.
                        whereClause +=
                            $"{columnName} = {prefix}{propertyName} AND ";
                        cmd.AddParameter(propertyName, value);
                        continue;
                    }

                    columnName = propertyMapping.ColumnName;
                    propertyName = propertyMapping.PropertyName;
                    try
                    {
                        value = propertyMapping.PropertyToColumnAdapter(kvp.Value);
                    }
                    catch (InvalidCastException exception)
                    {
                        throw new MappingException(typeof(TEntity), "Failed to cast '" + kvp.Key + "' from '" + kvp.Value.GetType() + "'.", exception);
                    }
                }

                whereClause +=
                    $"{columnName} = {prefix}{propertyName} AND ";
                cmd.AddParameter(propertyName, value);
            }

            if (string.IsNullOrEmpty(whereClause))
                return;

            cmd.CommandText += whereClause.Remove(whereClause.Length - 5, 5);
        }

        public static void ApplyQuerySql<TEntity>(this IDbCommand cmd, string sql,
            params object[] parameters)
        {
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            ApplyQuerySql(cmd, mapping, sql, parameters);
        }

        /// <summary>
        /// Builds a command using query or a short-hand query, and query parameters.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to load, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to add parameters to (should end with " WHERE " so that this method can add the constraints properly)</param>
        /// <param name="mapper">Mapper to use to convert properties to columns</param>
        /// <param name="sql">Complete (<code>"SELECT * FROM user WHERE id = @id"</code>) or short (<code>"id = @id"</code>).</param>
        /// <param name="parameters">Anonymous object (<code>new { id = user.Id }</code>), a dictionary or an array of values</param>
        /// <remarks>
        /// <para>
        /// Query 
        /// </para>
        /// 
        /// </remarks>
        /// <example>
        /// <para>Using complete query, with named arguments</para>
        /// <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         command.ApplyQuerySql("SELECT * FROM Users WHERE Id = @id", new { id = user.Id});
        ///         return cmd.First<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// <para>Using complete query, with array of values</para>
        /// <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         command.ApplyQuerySql("SELECT * FROM Users WHERE Id = @1", user.Id);
        ///         return cmd.First<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// <para>Using short query and named parameters</para>
        /// <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         command.ApplyQuerySql("Age <= @age AND City = @city", new { age = dto.Age, city = dto.City});
        ///         return cmd.ToList<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// <para>Using short query and a value array</para>
        /// <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         command.ApplyQuerySql("Age <= @1 AND City = @2", dto.Age, dto.City);
        ///         return cmd.First<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static void ApplyQuerySql<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper, string sql, params object[] parameters)
        {
            ApplySelectQuery(cmd, sql, mapper);
            if (parameters.Length == 0)
                return;

            var isSingleValueArray = parameters.Length == 1 && sql.Contains("@1");
            if (isSingleValueArray || parameters.Length > 1)
            {
                for (var i = 1; i <= parameters.Length; i++)
                {
                    cmd.AddParameter(i.ToString(CultureInfo.InvariantCulture), parameters[i - 1]);
                }
                return;
            }

            IDictionary<string, object> dictionary;
            switch (parameters[0])
            {
                case IDictionary<string, object> objects:
                    dictionary = objects;
                    break;
                case IDictionary dict:
                    dictionary = new Dictionary<string, object>(dict.Count);
                    foreach (var key in dict.Keys)
                        dictionary.Add(key.ToString(), dict[key]);
                    break;
                default:
                    dictionary = parameters[0].ToDictionary();
                    break;
            }

            var crudMapper = mapper as ICrudEntityMapper<TEntity>;
            foreach (var kvp in dictionary)
            {
                // do NOT change to property name here
                // as user have manually specified their own parameter name
                var propertyName = kvp.Key;
                var value = kvp.Value;

                // We can use custom names, thus we might not get a match.
                if (crudMapper != null && crudMapper.Properties.TryGetValue(kvp.Key, out var propertyMapping))
                {
                    try
                    {
                        value = propertyMapping.PropertyToColumnAdapter(kvp.Value);
                    }
                    catch (InvalidCastException exception)
                    {
                        throw new MappingException(typeof(TEntity), "Failed to cast '" + kvp.Key + "' from '" + kvp.Value.GetType() + "'.", exception);
                    }
                }

                cmd.AddParameter(propertyName, value);
            }
        }

        /// <summary>
        ///     Fetches the first row from a query, but mapped as an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <returns>Entity</returns>
        /// <exception cref="EntityNotFoundException">Failed to find entity</exception>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return cmd.First<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity First<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var result = cmd.FirstOrDefault<TEntity>();
            if (EqualityComparer<TEntity>.Default.Equals(result, default(TEntity)))
            {
                throw new EntityNotFoundException("Failed to find entity of type '" + typeof(TEntity).FullName + "'.",
                    cmd);
            }

            return result;
        }

        /// <summary>
        ///     Fetches the first row from a query, but mapped as an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <param name="mapper">Mapper which can convert the db row to an entity.</param>
        /// <returns>Entity</returns>
        /// <exception cref="EntityNotFoundException">Failed to find entity</exception>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void GetUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return cmd.First<User>(new MyCustomMapper());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity First<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var result = cmd.FirstOrDefault(mapper);
            if (EqualityComparer<TEntity>.Default.Equals(result, default(TEntity)))
            {
                throw new EntityNotFoundException("Failed to find entity of type '" + typeof(TEntity).FullName + "'.",
                    cmd);
            }

            return result;
        }

        /// <summary>
        ///     Fetches the first row and maps it as an entity (if found).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <returns>Entity if found; otherwise <c>null</c>.</returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void FindUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return cmd.FirstOrDefault<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(TEntity);


                    var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
                    var entity = (TEntity)mapping.Create(reader);
                    mapping.Map(reader, entity);
                    return entity;
                }
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }

        /// <summary>
        ///     Fetches the first row and maps it as an entity (if found).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <param name="mapper">Mapper which can convert the db row to an entity.</param>
        /// <returns>Entity if found; otherwise <c>null</c>.</returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void FindUser(string id)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return cmd.FirstOrDefault<User>(new MyCustomUserMapper());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity FirstOrDefault<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(TEntity);

                    var entity = mapper.Create(reader);
                    mapper.Map(reader, entity);
                    return (TEntity)entity;
                }
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         command/datareader is
        ///         kept open until the enumerator is disposed. Hence it's important that you make sure that the enumerator is
        ///         disposed when you are
        ///         done with it.
        ///     </para>
        ///     <para>
        ///         Hence the different between this method and the <see cref="ToList{TEntity}(IDbCommand)" />
        ///         method is
        ///         that this one do not create a list in the memory with all entities. It's therefore perfect if you want to
        ///         process a large amount
        ///         of rows.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public TimeSpan CalculateWorkHours()
        /// {
        ///     int minutes = 0;
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        /// 
        ///         // can contain a large amount of rows without consuming memory
        ///         using (var incidents = cmd.ToEnumerable<Incident>())
        ///         {
        ///             foreach (var incident in incidents)
        ///             {
        ///                 if (!incident.IsStarted)
        ///                     continue;
        /// 
        ///                 var spentTime = incident.ReportedTime.Sum(x => x.TotalSpentTime);
        ///                 minutes += spentTime;
        ///             }
        ///         }
        ///     }
        /// 
        ///     return TimeSpan.FromMinutes(minutes);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            return ToEnumerable<TEntity>(cmd, false);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
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
        ///     var pagedUsers = cmd.ToEnumerable<User>().Skip(1000).Take(50).ToList();
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
        ///         <see cref="ToList{TEntity}(IDbCommand)" />.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public TimeSpan CalculateWorkHours()
        /// {
        ///     int minutes = 0;
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        /// 
        ///         // can contain a large amount of rows without consuming memory
        ///         using (var incidents = cmd.ToEnumerable<Incident>())
        ///         {
        ///             foreach (var incident in incidents)
        ///             {
        ///                 if (!incident.IsStarted)
        ///                     continue;
        /// 
        ///                 var spentTime = incident.ReportedTime.Sum(x => x.TotalSpentTime);
        ///                 minutes += spentTime;
        ///             }
        ///         }
        ///     }
        /// 
        ///     return TimeSpan.FromMinutes(minutes);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbCommand cmd, bool ownsConnection)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return ToEnumerable(cmd, ownsConnection, mapping);
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <param name="ownsConnection">
        ///     <c>true</c> if the connection should be disposed together with the command/datareader. See
        ///     remarks.
        /// </param>
        /// <param name="mapper">Mapper which convert a db row to an entity</param>
        /// <returns>Lazy loaded enumerator</returns>
        /// <remarks>
        ///     <para>
        ///         The returned enumerator will not map each row until it's requested. To be able to do that the
        ///         connection/command/datareader is
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
        ///     var pagedUsers = cmd.ToEnumerable<User>().Skip(1000).Take(50).ToList();
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
        ///         <see cref="ToList{TEntity}(IDbCommand)" />.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public TimeSpan CalculateWorkHours()
        /// {
        ///     int minutes = 0;
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        /// 
        ///         // can contain a large amount of rows without consuming memory
        ///         using (var incidents = cmd.ToEnumerable<Incident>())
        ///         {
        ///             foreach (var incident in incidents)
        ///             {
        ///                 if (!incident.IsStarted)
        ///                     continue;
        /// 
        ///                 var spentTime = incident.ReportedTime.Sum(x => x.TotalSpentTime);
        ///                 minutes += spentTime;
        ///             }
        ///         }
        ///     }
        /// 
        ///     return TimeSpan.FromMinutes(minutes);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> ToEnumerable<TEntity>(this IDbCommand cmd, bool ownsConnection,
            IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            try
            {
                var reader = cmd.ExecuteReader();
                return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapper, ownsConnection);
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }



        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <returns>A collection of entities, or an empty collection if no entities are found.</returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void FindByName(string firstName, string lastName)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE ";
        ///         if (lastName != null)
        ///         {
        ///             cmd.AddParameter("firstName", firstName + "%");
        ///             cmd.CommandText += "FirstName LIKE @firstName AND ";
        ///         }
        ///         if (lastName != null)
        ///         {
        ///             cmd.AddParameter("lastName", lastName + "%");
        ///             cmd.CommandText += "LastName LIKE @lastName AND ";
        ///         }
        /// 
        ///         cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
        ///         return cmd.ToList<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return ToList(cmd, mapping);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to use, must have an mapper registered in <see cref="EntityMappingProvider"/>.</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReader()</c> on.</param>
        /// <param name="mapper">Mapper to use when converting the rows to entities</param>
        /// <returns>A collection of entities, or an empty collection if no entities are found.</returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public void FindByName(string firstName, string lastName)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE ";
        ///         if (lastName != null)
        ///         {
        ///             cmd.AddParameter("firstName", firstName + "%");
        ///             cmd.CommandText += "FirstName LIKE @firstName AND ";
        ///         }
        ///         if (lastName != null)
        ///         {
        ///             cmd.AddParameter("lastName", lastName + "%");
        ///             cmd.CommandText += "LastName LIKE @lastName AND ";
        ///         }
        /// 
        ///         cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 4, 4);
        ///         return cmd.ToList<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IList<TEntity> ToList<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            try
            {
                var items = new List<TEntity>(10);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entity = mapper.Create(reader);
                        mapper.Map(reader, entity);
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
        /// Add a SELECT query to the command (support for short queries)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="command">Command to assign the query to (CommandText)</param>
        /// <param name="query">Complete query (starting with "SELECT") or a short one (just WHERE constraints)</param>
        /// <param name="mapper">Mapper, used for short queries</param>
        internal static void ApplySelectQuery<TEntity>(this IDbCommand command, string query, IEntityMapper<TEntity> mapper)
        {
            if (query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                command.CommandText = query;
                return;
            }

            var crudMapper = mapper.AsCrudMapper();
            command.CommandText = $"SELECT * FROM {crudMapper.TableName} WHERE {query}";
        }
    }
}