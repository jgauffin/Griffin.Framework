using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Sqlite.IntegrationTests.Entites;
using Xunit;

namespace Griffin.Data.Sqlite.IntegrationTests
{
    public class AsyncCommandExtensionsTests : IDisposable
    {
        private string _dbFile;
        private SQLiteConnection _connection;
        private List<User>  _users = new List<User>();
        private DateTime _dob;

        public AsyncCommandExtensionsTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));

            _dbFile = Path.GetTempFileName();
            string cs = "URI=file:" + _dbFile;
            _connection = new SQLiteConnection(cs);
            _connection.Open();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE Users (Id varchar(20) not null primary key, FirstName varchar(20), LastName varchar(20), DateOfBirth NUMERIC, MessageCount integer)";
                cmd.ExecuteNonQuery();
            }

            _dob = DateTime.Now;
            _dob=_dob.AddSeconds(0 - _dob.Second);

            using (var cmd = _connection.CreateCommand())
            {
                for (int i = 0; i < 50; i++)
                {
                    var user = new User
                    {
                        FirstName = "First" + i,
                        LastName = Guid.NewGuid().ToString(),
                        Id = Guid.NewGuid(),
                        DateOfBirth = _dob,
                        MessageCount = i + 10
                    };
                    _users.Add(user);
                    cmd.Insert(user);
                }
            }
        }

        [Fact]
        public async  Task First()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                var user = cmd.First<User>();

                user.FirstName.Should().Be(_users[0].FirstName);
                user.LastName.Should().Be(_users[0].LastName);
                user.DateOfBirth.Should().Be(_dob);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
            File.Delete(_dbFile);
        }
    }
}
