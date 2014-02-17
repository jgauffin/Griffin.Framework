using System;
using System.Collections.Generic;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class UserMapper : EntityMapper<User>
    {
        public UserMapper() : base("Users")
        {
        }

        /// <summary>
        ///     Used to map all properties which should be read from the database record.
        /// </summary>
        /// <param name="mappings">Dictionary which should be filled with all mappings</param>
        /// <remarks>
        ///     <para>
        ///         Will scan all properties and assign them a mapping, even if the setters are non-public. If no setter is
        ///         available
        ///         it will try to finding a field using the name convention where <c>FirstName</c> becomes <c>_firstName</c>.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <para>If you want to remove a property:</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings.Remove("CreatedAt");
        /// }
        /// ]]>
        /// </code>
        ///     <para>Example if the column type is <c>uniqueidentifier</c> and the property type is string:</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["Id"].ColumnToPropertyAdapter = value => value.ToString();
        /// }
        /// ]]>
        /// </code>
        ///     <para>If you have stored a child aggregate as a JSON string in a column</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["AuditLog"].ColumnToPropertyAdapter = value => JsonConvert.ToObject<IEnumerable<AuditEntry>>(value.ToString());
        /// }
        /// ]]>
        /// </code>
        ///     <para>To convert an int column in the db to an enum</para>
        ///     <code>
        /// <![CDATA[
        /// public override void Configure(IDictionary<string, PropertyMapping> mappings)
        /// {
        ///     base.Configure(mappings);
        ///     mappings["State"].ColumnToPropertyAdapter = value => (UserState)value;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public override void Configure(IDictionary<string, IPropertyMapping> mappings)
        {
            base.Configure(mappings);
            mappings["DateOfBirth"].ColumnToPropertyAdapter = x => ((double)x).FromUnixTime();
            mappings["DateOfBirth"].PropertyToColumnAdapter = x => ((DateTime)x).ToUnixTime();
        }
    }
}