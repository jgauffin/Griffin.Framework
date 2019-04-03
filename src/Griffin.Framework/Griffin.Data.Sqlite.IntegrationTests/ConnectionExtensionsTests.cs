using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Sqlite.IntegrationTests.Entites;
using Xunit;

namespace Griffin.Data.Sqlite.IntegrationTests
{
    public class ConnectionExtensionsTests
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly UserTable _userTable = new UserTable();

        public ConnectionExtensionsTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));
            var provider = new AssemblyScanningMappingProvider();
            provider.Scan(Assembly.GetExecutingAssembly());
            EntityMappingProvider.Provider = provider;


            //EntityMappingProvider.Provider = new AssemblyScanningMappingProvider();
            _dbFile = Path.GetTempFileName();
            var cs = "URI=file:" + _dbFile;
            _connection = new SQLiteConnection(cs);
            _connection.Open();

            _userTable.Create(_connection);
        }


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
            File.Delete(_dbFile);
        }


        [Fact]
        public void FirstOrDefault_with_objectConstraint_should_return_wanted_row()
        {
            _userTable.Insert(_connection, 50);

            var actual = _connection.FirstOrDefault<User>(new { _userTable.Users[11].FirstName });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public void Insert_row()
        {
            var expected = new User() { FirstName = "Arne", Id = Guid.NewGuid() };

            _connection.Insert(expected);

            var actual = _connection.First<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
            actual.Id.Should().Be(expected.Id);
        }



        [Fact]
        public void first_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[10];

            var actual = _connection.First<User>(new { Id = _userTable.Users[10].Id });

            actual.LastName.Should().Be(expected.LastName);
        }


        [Fact]
        public void truncate()
        {
            var msgs = new MessageTable();
            msgs.Create(_connection);
            msgs.Insert(_connection, 50);

            _connection.Truncate<Message>();
            var actual = _connection.FirstOrDefault<Message>(new { msgs.Items[10].Id });

            actual.Should().BeNull();
        }

        [Fact]
        public void execute()
        {
            _userTable.Insert(_connection, 50);

            _connection.ExecuteNonQuery("UPDATE Users SET FirstName=@name", new {name = "Kalle"});
            var actual = _connection.FirstOrDefault<User>(new { _userTable.Users[10].Id });

            actual.FirstName.Should().Be("Kalle");
        }


        [Fact]
        public void delete_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            _connection.Delete(expected);

            var actual = _connection.FirstOrDefault<User>(new { expected.Id });
            actual.Should().BeNull();
        }

        [Fact]
        public void delete_user_using_constraints()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            _connection.Delete<User>(new { expected.Id });

            var actual = _connection.FirstOrDefault<User>(new { expected.Id });
            actual.Should().BeNull();
        }


        [Fact]
        public void update_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            expected.FirstName = "Jonas";
            _connection.Update<User>(expected);

            var actual = _connection.First<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
        }
    }
}
