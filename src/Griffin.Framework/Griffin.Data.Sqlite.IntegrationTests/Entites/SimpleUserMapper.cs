using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class SimpleUserMapper : CrudEntityMapper<SimpleUser>
    {
        public SimpleUserMapper() : base("SimpleUsers")
        {
            Property(x => x.Id)
                .PrimaryKey(true);
        }
    }
}