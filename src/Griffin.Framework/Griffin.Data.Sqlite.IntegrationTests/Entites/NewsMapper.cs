using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    class NewsMapper : CrudEntityMapper<News>
    {
        public NewsMapper() : base("News")
        {
            Property(x => x.Id)
                .PrimaryKey(true);
        }
    }
}
