using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    class EmptyMapping : EntityMapper<Empty>
    {
        public EmptyMapping() : base("Emty")
        {
        }
    }
}
