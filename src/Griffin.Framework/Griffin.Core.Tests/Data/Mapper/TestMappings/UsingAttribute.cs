using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Mapper.Values;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    
    class UsingAttribute
    {
    }

    [MappingFor(typeof(UsingAttribute))]
    class UsingAttributeMapper : ICrudEntityMapper
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
        public object Create(IDataRecord record)
        {
            throw new NotImplementedException();
        }

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
        public void Map(IDataRecord source, object destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Free the mapping, no further changes may be made.
        /// </summary>
        /// <remarks>
        /// <para>Called by the mapping provider when it's being added to it.</para>
        /// </remarks>
        public void Freeze()
        {
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// All properties in this mapping
        /// </summary>
        public IDictionary<string, IPropertyMapping> Properties { get; private set; }

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
        public KeyValuePair<string, object>[] GetKeys(object entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to create SQL commands which is specific for this entity.
        /// </summary>
        /// <remarks>
        /// <para>The recommended approach for implementations is to retrieve the command builder from <see cref="CommandBuilderFactory"/> when the <c>Freeze()</c> method is being invoked.
        /// By doing so it's easy to adapt and precompile the command strings and logic before any invocations is made.
        /// </para>
        /// </remarks>
        public ICommandBuilder CommandBuilder { get; private set; }

        IDictionary<string, IPropertyMapping> ICrudEntityMapper.Properties => throw new NotImplementedException();
    }

}
