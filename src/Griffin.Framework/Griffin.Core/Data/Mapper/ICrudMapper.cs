using System.Data;

namespace Griffin.Data.Mapper
{
    public interface ICrudMapper : IEntityMapper
    {
        void MapInsert(object entity, IDbCommand cmd);
    }
}