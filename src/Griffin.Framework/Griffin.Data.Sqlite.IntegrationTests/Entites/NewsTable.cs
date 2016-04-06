using System.Collections.Generic;
using System.Data.SQLite;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class NewsTable
    {
        public List<News> News { get; } = new List<News>();

        public void Create(SQLiteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE News (Id integer not null primary key, Title varchar(20))";
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete()
        {
        }

        public void Insert(SQLiteConnection connection, int numberOfItems)
        {
            for (var i = 0; i < numberOfItems; i++)
            {
                var news = new News
                {
                    Title = "Headline " + i
                };

                News.Add(news);
                connection.Insert(news);
            }
        }
    }
}