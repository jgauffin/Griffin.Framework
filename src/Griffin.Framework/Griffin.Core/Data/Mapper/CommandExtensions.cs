using System;
using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Extensions for <see cref="IDbCommand" />.
    /// </summary>
    public static class CommandExtensions
    {
        public static void Insert<TEntity>(this IDbCommand cmd, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            mapper.CommandBuilder.InsertCommand(cmd, entity);
            cmd.ExecuteNonQuery();
        }

        public static void Update<TEntity>(this IDbCommand cmd, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            mapper.CommandBuilder.UpdateCommand(cmd, entity);
            cmd.ExecuteNonQuery();
        }

        public static void Delete<TEntity>(this IDbCommand cmd, TEntity entity)
        {
            var mapper = EntityMappingProvider.GetMapper<TEntity>();
            mapper.CommandBuilder.DeleteCommand(cmd, entity);
            cmd.ExecuteNonQuery();
        }


        /// <summary>
        ///     Fetches the first row from a query, but mapped as an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to fetch</typeparam>
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
                throw new EntityNotFoundException("Failed to find entity of type '" + typeof (TEntity).FullName + "'.",
                    cmd);
            }

            return result;
        }

        /// <summary>
        ///     Fetches the first row from a query, but mapped as an entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to fetch</typeparam>
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
                throw new EntityNotFoundException("Failed to find entity of type '" + typeof (TEntity).FullName + "'.",
                    cmd);
            }

            return result;
        }

        /// <summary>
        ///     Fetches the first row and maps it as an entity (if found).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to fetch</typeparam>
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

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return default(TEntity);


                var mapping = EntityMappingProvider.GetMapper<TEntity>();
                var entity = (TEntity) mapping.Create(reader);
                mapping.Map(reader, entity);
                return entity;
            }
        }

        /// <summary>
        ///     Fetches the first row and maps it as an entity (if found).
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to fetch</typeparam>
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

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return default(TEntity);

                var entity = mapper.Create(reader);
                mapper.Map(reader, entity);
                return (TEntity) entity;
            }
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
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
        ///         Hence the different between this method and the <see cref="ToList{TEntity}(System.Data.Common.DbCommand)" />
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
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
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
        ///         <see cref="ToList{TEntity}" />.
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

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            return ToEnumerable(cmd, ownsConnection, mapping);
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
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
        ///         <see cref="ToList{TEntity}" />.
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

            var reader = cmd.ExecuteReader();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapper, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
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

            var mapping = EntityMappingProvider.GetMapper<TEntity>();
            return ToList(cmd, mapping);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
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

            var items = new List<TEntity>(10);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var entity = mapper.Create(reader);
                    mapper.Map(reader, entity);
                    items.Add((TEntity) entity);
                }
            }
            return items;
        }
    }
}