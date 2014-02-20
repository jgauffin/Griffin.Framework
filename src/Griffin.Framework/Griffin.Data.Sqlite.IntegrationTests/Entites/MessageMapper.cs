using System;
using System.Collections.Generic;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class MessageMapper : EntityMapper<Message>
    {
        public MessageMapper()
            : base("Messages")
        {
        }
        public override void Configure(IDictionary<string, IPropertyMapping> mappings)
        {
            base.Configure(mappings);
            mappings["ReceivedAt"].ColumnToPropertyAdapter = x =>
            {
                if (x is DBNull)
                    return DateTime.MinValue;

                return (Convert.ToInt32(x)).FromUnixTime();
            };
            mappings["ReceivedAt"].PropertyToColumnAdapter = x =>
            {
                if (DateTime.MinValue.Equals(x))
                    return DBNull.Value;

                return ((DateTime)x).ToUnixTime();
            };
        }
    }
}