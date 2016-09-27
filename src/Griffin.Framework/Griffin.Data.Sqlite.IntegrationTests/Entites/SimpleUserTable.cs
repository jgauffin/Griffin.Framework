using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class SimpleUserTable
    {
        public List<SimpleUser> Users { get; } = new List<SimpleUser>();

        public void Create(SQLiteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE SimpleUsers (Id INTEGER PRIMARY KEY autoincrement, FirstName varchar(20))";
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete()
        {
        }

        public void Insert(SQLiteConnection connection, int numberOfUsers)
        {
            var r = new Random();
            for (var i = 0; i < numberOfUsers; i++)
            {
                var user = new SimpleUser
                {
                    FirstName = "First" + i,
                };

                Users.Add(user);

                connection.Insert(user);
            }
        }
    }
}