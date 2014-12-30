using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Asynchronous extensions for <see cref="DbCommand" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All methods which do not take a mapper class uses the <see cref="EntityMappingProvider" /> to identify the
    ///         mapper to use when converting to/from rows in the database. SQL commands
    ///         for CRUD operations are provided by a <see cref="ICommandBuilder" /> implementation (specific for each database
    ///         engine).
    ///     </para>
    ///     <para>
    ///         CRUD operations are typically performed on the <see cref="IAdoNetUnitOfWork" /> or <see cref="IDbConnection" />
    ///         instead as you do not have to create your own command then.
    ///     </para>
    /// </remarks>
    public static class AsyncCommandExtensions
    {
        /// <summary>
        ///     Fetches the first found entity asynchronously
        /// </summary>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <returns>
        ///     entity
        /// </returns>
        /// <exception cref="EntityNotFoundException">Failed to find specified entity.</exception>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        ///     <para>
        ///         Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return await cmd.FirstAsync<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Task<TEntity> FirstAsync<TEntity>(this IDbCommand cmd)
        {
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return FirstAsync(cmd, mapping);
        }

        /// <summary>
        ///     Fetches the first found entity asynchronously
        /// </summary>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <param name="mapper"></param>
        /// <returns>
        ///     entity
        /// </returns>
        /// <exception cref="EntityNotFoundException">Failed to find entity</exception>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return await cmd.FirstAsync<User>(new MyCustomMapper());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="CrudEntityMapper{TEntity}" />
        public static async Task<TEntity> FirstAsync<TEntity>(this IDbCommand cmd, IEntityMapper<TEntity> mapper)
        {
            var result = await cmd.FirstOrDefaultAsync(mapper);
            if (EqualityComparer<TEntity>.Default.Equals(result, default(TEntity)))
            {
                throw new EntityNotFoundException("Failed to find entity of type '" + typeof (TEntity).FullName + "'.",
                    cmd);
            }

            return result;
        }

        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <returns>
        ///     Entity if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return await cmd.FirstOrDefaultAsync<User>();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return FirstOrDefaultAsync(cmd, mapping);
        }


        /// <summary>
        ///     Fetches the first row if found.
        /// </summary>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <param name="mapper">Mapper used to convert rows to entities</param>
        /// <returns>
        ///     Entity if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     <para>Use this method when an entity is expected to be returned.</para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// public async Task<User> GetUser(int userId)
        /// {
        ///     using (var command = connection.CreateCommand())
        ///     {
        ///         cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        ///         cmd.AddParameter("id", userId);
        ///         return await cmd.FirstOrDefaultAsync<User>(new MyCustomMapping());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="CrudEntityMapper{TEntity}" />
        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(this IDbCommand cmd,
            IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var command = GetDbCommandFromInterface(cmd);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    return default(TEntity);

                var entity = (TEntity) mapper.Create(reader);
                mapper.Map(reader, entity);
                return entity;
            }
        }

        private static DbCommand GetDbCommandFromInterface(IDbCommand cmd)
        {
            var command = cmd as DbCommand;
            if (command == null)
                throw new NotSupportedException(
                    "Microsoft didn't create a new interface for asynchronous operations and your implementation of IDbCommand do not inherit DbCommand which is required for async features.");
            return command;
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
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
        ///         If the result returned from the query is all records that you want it's probably more efficient to use
        ///         <see cref="ToListAsync{TEntity}(System.Data.Common.DbCommand)" />.
        ///     </para>
        ///     <para>Uses <see cref="EntityMappingProvider" /> to find the correct <c><![CDATA[IEntityMapper<TEntity>]]></c>.</para>
        /// </remarks>
        public static Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            return ToEnumerableAsync<TEntity>(cmd, false);
        }

        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
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
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbCommand cmd,
            bool ownsConnection)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var command = GetDbCommandFromInterface(cmd);
            var reader = await command.ExecuteReaderAsync();
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Return an enumerable which uses lazy loading of each row.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
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
        public static async Task<IEnumerable<TEntity>> ToEnumerableAsync<TEntity>(this IDbCommand cmd,
            bool ownsConnection, IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var command = GetDbCommandFromInterface(cmd); 
            var reader = await command.ExecuteReaderAsync();
            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return new AdoNetEntityEnumerable<TEntity>(cmd, reader, mapping, ownsConnection);
        }


        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         Uses the <see cref="EntityMappingProvider" /> to find the correct base mapper.
        ///     </para>
        ///     <para>
        ///         Make sure that you <c>await</c> the method, as nothing the reader is not disposed directly if you don't.
        ///     </para>
        /// </remarks>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var mapping = EntityMappingProvider.GetBaseMapper<TEntity>();
            return await ToListAsync(cmd, mapping);
        }

        /// <summary>
        ///     Generate a complete list before returning.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to map</typeparam>
        /// <param name="cmd">Command to invoke <c>ExecuteReaderAsync()</c> on.</param>
        /// <param name="mapper">Mapper to use when converting rows to entities</param>
        /// <returns>A list which is generated asynchronously.</returns>
        /// <remarks>
        ///     <para>
        ///         Make sure that you <c>await</c> the method, as nothing the reader is not disposed directly if you don't.
        ///     </para>
        /// </remarks>
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this IDbCommand cmd,
            IEntityMapper<TEntity> mapper)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (mapper == null) throw new ArgumentNullException("mapper");

            var items = new List<TEntity>();
            var command = GetDbCommandFromInterface(cmd);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
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