using System;
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
    public class AsyncConnectionExtensionsTests
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly UserTable _userTable = new UserTable();

        public AsyncConnectionExtensionsTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));

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
        public async Task FirstOrDefault_with_objectConstraint_should_return_wanted_row()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>(new { _userTable.Users[11].FirstName });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task Insert_row()
        {
            var expected = new User() { FirstName = "Arne", Id = Guid.NewGuid() };

            await _connection.InsertAsync(expected);

            var actual = await _connection.FirstAsync<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
            actual.Id.Should().Be(expected.Id);
        }



        [Fact]
        public async Task first_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[10];

            var actual = await _connection.FirstAsync<User>(new { _userTable.Users[10].Id });

            actual.LastName.Should().Be(expected.LastName);
        }




        [Fact]
        public async Task delete_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _connection.DeleteAsync(expected);

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }

        [Fact]
        public async Task delete_user_using_constraints()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _connection.DeleteAsync<User>(new { expected.Id });

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }


        [Fact]
        public async Task update_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            expected.FirstName = "Jonas";
            await _connection.UpdateAsync(expected);

            var actual = await _connection.FirstAsync<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
        }
    }
}
