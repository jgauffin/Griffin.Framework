using System;
using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    internal class EntityWithCoupledFieldsMapper : CrudEntityMapper<EntityWithCoupledFields>
    {
        public EntityWithCoupledFieldsMapper() : base("MyTable")
        {
            Property(x => x.ValueType)
                .ToColumnValue2(x => x.Value.GetType().FullName);

            Property(x => x.Value)
                .ToColumnValue(x => x.ToString())
                .ToPropertyValue2(x =>
                {
                    var type = Type.GetType((string) x.Record["ValueType"], true);
                    return Convert.ChangeType(x.Value, type);
                });
        }
    }
}