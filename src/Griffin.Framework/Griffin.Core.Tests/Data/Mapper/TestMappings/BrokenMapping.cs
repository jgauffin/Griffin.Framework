using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    class BrokenMapping : IEntityMapper
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
            throw new NotImplementedException();
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
        /// Used to create SQL commands which is specific for this entity.
        /// </summary>
        /// <remarks>
        /// <para>The recommended approach for implementations is to retrieve the command builder from <see cref="CommandBuilderFactory"/> when the <c>Freeze()</c> method is being invoked.
        /// By doing so it's easy to adapt and precompile the command strings and logic before any invocations is made.
        /// </para>
        /// </remarks>
        public ICommandBuilder CommandBuilder { get; private set; }

       
    }
}
