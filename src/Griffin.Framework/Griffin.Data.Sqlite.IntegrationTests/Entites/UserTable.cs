using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class UserTable
    {
        private readonly List<User> _users = new List<User>();

        public List<User> Users
        {
            get { return _users; }
        }

        public void Create(SQLiteConnection connection)
        {

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE Users (Id varchar(36) not null primary key, FirstName varchar(20), LastName varchar(20), DateOfBirth NUMERIC, MessageCount integer)";
                cmd.ExecuteNonQuery();
            }
        }

        public void Insert(SQLiteConnection connection, int numberOfUsers)
        {
            Random r = new Random();
            for (int i = 0; i < numberOfUsers; i++)
            {
                var user = new User
                {
                    FirstName = "First" + i,
                    LastName = Guid.NewGuid().ToString(),
                    Id = Guid.NewGuid(),
                    DateOfBirth = DateTime.Today.AddDays(r.Next(-100, 100)),
                    MessageCount = i + 10
                };

                Users.Add(user);

                connection.Insert(user);
            }
        }

        public void Delete()
        {

        }
    }
}