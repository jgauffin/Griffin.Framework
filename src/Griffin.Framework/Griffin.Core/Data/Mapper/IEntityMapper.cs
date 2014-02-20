using System.Collections.Generic;
using System.Data;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Maps a table column to a .NET entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Important! The implementations of this interface should be considered to be singletons. Hence any state in them
    /// must be thread safe. The same instance can be used to map multiple entities at the same time.
    /// </para>
    /// <para>
    /// You have to decorate class that implement this interface with the <see cref="MappingForAttribute"/> to tell the scanner
    /// which entity the class is a mapping for.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [MappingFor(typeof(User))
    /// class UserMapper : IEntityMapper<User>
    /// {
    ///     public object Create(IDataRecord source)
    ///     {
    ///         return new User();
    ///     }
    /// 
    ///     public void Map(IDataRecord source, object destination)
    ///     {
    ///         var user = (User)destination;
    ///         user.Id = source["Id"].ToString();
    ///         user.Age = (int)source["Age"];
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IEntityMapper
    {
        /// <summary>
        /// Create a new entity for the specified 
        /// </summary>
        /// <param name="record">should only be used to initialize any constructor arguments.</param>
        /// <returns>Created entity</returns>
        /// <example>
        /// <para>Where a default constructor exists:</para>
        /// <code>
        /// public object Create(IDataRecord record)
        /// {
        ///     return new User();
        /// }
        /// </code>
        /// <para>Where a only constructors with arguments exists:</para>
        /// <code>
        /// public object Create(IDataRecord record)
        /// {
        ///     return new User(record["Id"].ToString());
        /// }
        /// </code>
        /// </example>
        object Create(IDataRecord record);

        /// <summary>
        /// Get mapping for a 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// TODO: Fix
        //PropertyMapping GetMapping(string name);

        /// <summary>
        /// Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        /// <example>
        /// <code>
        /// public void Map(IDataRecord source, object destination)
        /// {
        ///     var user = (User)destination;
        ///     user.Id = source["Id"].ToString();
        ///     user.Age = (int)source["Age"];
        /// }
        /// </code>
        /// </example>
        void Map(IDataRecord source, object destination);

        /// <summary>
        /// Free the mapping, no further changes may be made.
        /// </summary>
        /// <remarks>
        /// <para>Called by the mapping provider when it's being added to it.</para>
        /// </remarks>
        void Freeze();

        /// <summary>
        /// Gets table name
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// All properties in this mapping
        /// </summary>
        IDictionary<string, IPropertyMapping> Properties { get; }

        /// <summary>
        /// Get the primary key 
        /// </summary>
        /// <param name="entity">Entity to fetch key values from</param>
        /// <returns>A single item in the array for a single PK column and one entry per column in composite primary key</returns>
        /// <example>
        /// <para>If you have a single primary key (like an auto incremented column)</para>
        /// <code>
        /// <![CDATA[
        /// var user = new User { Id = 24, Name = "Jonas" };
        /// var mapping = new EntityMapping<User>();
        /// var pk = mapping.GetKeys(user);
        /// 
        /// Console.WriteLine(pk[0].Name + " = " + pk[0].Value); // prints "Id = 24"
        /// ]]>
        /// </code>
        /// <para>
        /// A composite key:
        /// </para>
        /// <code>
        /// <![CDATA[
        /// var address = new UserAddress{ UserId = 24, ZipCode  = "1234", City = "Falun" };
        /// var mapping = new EntityMapping<UserAddress>();
        /// var pk = mapping.GetKeys(address);
        /// 
        /// Console.WriteLine(pk[0].Value + ", " + pk[1].Value); // prints "24, 1234"
        /// ]]>
        /// </code>
        /// </example>
        KeyValuePair<string,object>[] GetKeys(object entity);

        /// <summary>
        /// Used to create SQL commands which is specific for this entity.
        /// </summary>
        /// <remarks>
        /// <para>The recommended approach for implementations is to retrieve the command builder from <see cref="CommandBuilderFactory"/> when the <c>Freeze()</c> method is being invoked.
        /// By doing so it's easy to adapt and precompile the command strings and logic before any invocations is made.
        /// </para>
        /// </remarks>
        ICommandBuilder CommandBuilder { get; }
    }

    /// <summary>
    /// Generic version of the mapping method to make it easier to do mappings manually.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <remarks>
    /// <para>
    /// Important! The implementations of this interface should be considered to be singletons. Hence any state in them
    /// must be thread safe. The same instance can be used to map multiple entities at the same time.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public class UserMapper : IEntityMapper<User>
    /// {
    ///     public object Create(IDataRecord source)
    ///     {
    ///         return new User();
    ///     }
    /// 
    ///     public void Map(IDataRecord source, User destination)
    ///     {
    ///         destination.Id = source["Id"].ToString();
    ///         destination.Age = (int)source["Age"];
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IEntityMapper<in TEntity> : IEntityMapper
    {
        /// <summary>
        /// Map a record to the specified entity
        /// </summary>
        /// <param name="source">Record from the DB</param>
        /// <param name="destination">Entity to fill with information</param>
        /// <example>
        /// <code>
        /// public void Map(IDataRecord source, User destination)
        /// {
        ///     destination.Id = source["Id"].ToString();
        ///     destination.Age = (int)source["Age"];
        /// }
        /// </code>
        /// </example>
        void Map(IDataRecord source, TEntity destination);
    }

}
