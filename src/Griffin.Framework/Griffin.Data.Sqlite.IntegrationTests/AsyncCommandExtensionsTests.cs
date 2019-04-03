using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
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
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly UserTable _userTable = new UserTable();

        public AsyncCommandExtensionsTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));
            var provider = new AssemblyScanningMappingProvider();
            provider.Scan(Assembly.GetExecutingAssembly());
            EntityMappingProvider.Provider = provider;
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
        public async Task First_with_rows_should_succeed()
        {
            _userTable.Insert(_connection, 50);

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                var user = await cmd.FirstAsync<User>();

                user.FirstName.Should().Be(_userTable.Users[0].FirstName);
                user.LastName.Should().Be(_userTable.Users[0].LastName);
                user.DateOfBirth.Should().Be(_userTable.Users[0].DateOfBirth);
            }
        }

        [Fact]
        public void First_without_rows_should_throw_exception()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users WHERE Id = 8338";
                Action actual = () => cmd.FirstAsync<User>().Wait();

                actual.Should().Throw<EntityNotFoundException>();
            }
        }

        [Fact]
        public async Task FirstOrDefault_without_rows_should_return_default()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                var actual = await cmd.FirstOrDefaultAsync<User>();

                actual.Should().BeNull();
            }
        }

        [Fact]
        public async Task FirstOrDefault_without_one_row_should_return_that_row()
        {
            _userTable.Insert(_connection, 50);

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users WHERE FirstName = @someName";
                cmd.AddParameter("someName", "First1");
                var actual = await cmd.FirstOrDefaultAsync<User>();

                actual.Should().NotBeNull();
                actual.LastName.Should().Be(_userTable.Users[1].LastName);
            }
        }

        [Fact]
        public async Task enumerate()
        {
            _userTable.Insert(_connection, 50);
            int counter = 0;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                var users = await cmd.ToEnumerableAsync<User>();
                foreach (var user in users)
                {
                    Console.WriteLine(user.FirstName);
                    counter ++;
                }
            }

            counter.Should().Be(50);
        }

        [Fact]
        public async Task enumerate_no_rows()
        {
            int counter = 0;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                var users = await cmd.ToEnumerableAsync<User>();
                foreach (var user in users)
                {
                    Console.WriteLine(user.FirstName);
                    counter++;
                }
            }

            counter.Should().Be(0);
        }

        [Fact]
        public async Task ToList_should_return_rows()
        {
            _userTable.Insert(_connection, 50);
            IList<User> users;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                users = await cmd.ToListAsync<User>();
            }

            users.Count.Should().Be(50);
        }

        [Fact]
        public async Task tolist_no_rows()
        {
            IList<User> users;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                users = await cmd.ToListAsync<User>();
            }

            users.Count.Should().Be(0);
        }
    }
}