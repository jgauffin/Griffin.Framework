using System;
using System.Collections.Generic;
using Griffin.Core.Data.Mapper;

namespace Sqlite
{
    public class UserMapper : EntityMapper<User>
    {
        private static readonly DateTime UnixDate = new DateTime(1970, 1, 1);

        public UserMapper() : base("Users")
        {
        }

        public override void Configure(IDictionary<string, IPropertyMapping> mappings)
        {
            base.Configure(mappings);
            mappings["Id"].ColumnToPropertyAdapter = i => Convert.ToInt32(i);
            mappings["CreatedAtUtc"].ColumnToPropertyAdapter = o => UnixDate.AddSeconds(Convert.ToInt32(o));
            mappings["CreatedAtUtc"].PropertyToColumnAdapter = o => ((DateTime) o).Subtract(UnixDate).TotalSeconds;
        }
    }
}