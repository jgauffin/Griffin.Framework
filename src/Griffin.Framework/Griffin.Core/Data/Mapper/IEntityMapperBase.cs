using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Contract for only mapping rows and nothing more.
    /// </summary>
    public interface IEntityMapperBase
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
    public interface IEntityMapperBase<in TEntity> : IEntityMapperBase
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