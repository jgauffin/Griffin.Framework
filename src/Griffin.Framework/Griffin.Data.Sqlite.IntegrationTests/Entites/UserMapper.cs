using System;
using System.Collections.Generic;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class UserMapper : CrudEntityMapper<User>
    {
        public UserMapper() : base("Users")
        {
        }

        public override void Configure(IDictionary<string, IPropertyMapping> mappings)
        {
            base.Configure(mappings);
            mappings["Id"].ColumnToPropertyAdapter = x => Guid.Parse((string) x);
            mappings["Id"].PropertyToColumnAdapter = x => ((Guid) x).ToString("N");
            mappings["DateOfBirth"].ColumnToPropertyAdapter = x =>
            {
                if (x is DBNull)
                    return DateTime.MinValue;

                return (Convert.ToInt32(x)).FromUnixTime();
            };
            mappings["DateOfBirth"].PropertyToColumnAdapter = x =>
            {
                if (DateTime.MinValue.Equals(x))
                    return DBNull.Value;

                return ((DateTime) x).ToUnixTime();
            };
        }
    }
}