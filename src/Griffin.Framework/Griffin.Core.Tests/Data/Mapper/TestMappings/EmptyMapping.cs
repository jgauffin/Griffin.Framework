using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    class EmptyMapping : CrudEntityMapper<Empty>
    {
        public EmptyMapping() : base("Emty")
        {
        }
    }
}
