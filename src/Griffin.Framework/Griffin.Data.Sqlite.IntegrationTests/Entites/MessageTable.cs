using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class MessageTable
    {
        private readonly List<Message> _users = new List<Message>();

        public List<Message> Items
        {
            get { return _users; }
        }

        public void Create(SQLiteConnection connection)
        {

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE Messages (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title varchar(20), Body varchar(20200), ReceivedAt NUMERIC)";
                cmd.ExecuteNonQuery();
            }
        }

        public void Insert(SQLiteConnection connection, int count)
        {
            Random r = new Random();
            for (int i = 0; i < count; i++)
            {
                var item = new Message
                {
                    Title = "Ttl" + i,
                    Body = "Hello world " + Guid.NewGuid().ToString(),
                    ReceivedAt = DateTime.Now.AddSeconds(r.Next(-10000, 0)),
                };

                Items.Add(item);

                connection.Insert(item);
            }
        }

        public void Delete()
        {

        }
    }
}