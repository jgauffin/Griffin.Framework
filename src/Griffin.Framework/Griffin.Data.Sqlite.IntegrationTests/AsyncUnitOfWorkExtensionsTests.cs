using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Sqlite.IntegrationTests.Entites;
using Xunit;

namespace Griffin.Data.Sqlite.IntegrationTests
{
    public class AsyncUnitOfWorkExtensionsTests
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly UserTable _userTable = new UserTable();
        private AdoNetUnitOfWork _uow;

        public AsyncUnitOfWorkExtensionsTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));
            var provider = new AssemblyScanningMappingProvider();
            provider.Scan(Assembly.GetExecutingAssembly());
            EntityMappingProvider.Provider = provider;

            _dbFile = Path.GetTempFileName();
            var cs = "URI=file:" + _dbFile;
            _connection = new SQLiteConnection(cs);
            _connection.Open();

            _uow = new AdoNetUnitOfWork(_connection);
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
        public async Task Insert_row()
        {
            var expected = new User() { FirstName = "Arne", Id = Guid.NewGuid() };

            await _uow.InsertAsync(expected);
            _uow.SaveChanges();

            var actual = await _connection.FirstAsync<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
            actual.Id.Should().Be(expected.Id);

        }

        [Fact]
        public async Task first_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[10];

            var actual = await _uow.FirstAsync<User>(new { _userTable.Users[10].Id });

            actual.LastName.Should().Be(expected.LastName);
        }

        [Fact]
        public async Task First_with_short_query()
        {
            _userTable.Insert(_connection, 50);

            var sql = "FirstName = @name";
            var users = await _uow.ToListAsync<User>(sql, new { name = _userTable.Users[0].FirstName });

            users.Count.Should().BeGreaterThan(0);
        }



        [Fact]
        public async Task delete_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _uow.DeleteAsync(expected);
            _uow.SaveChanges();

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }

        [Fact]
        public async Task delete_user_using_constraints()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _uow.DeleteAsync<User>(new { expected.Id });
            _uow.SaveChanges();

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }


        [Fact]
        public async Task update_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            expected.FirstName = "Jonas";
            await _uow.UpdateAsync(expected);

            var actual = await _connection.FirstAsync<User>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
        }


        [Fact]
        public async Task FirstOrDefault_with_objectConstraint_should_return_wanted_row()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>(new { _userTable.Users[11].FirstName });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = @Id", new { _userTable.Users[11].Id });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task FirstOrDefault_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_short_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("id = @1", _userTable.Users[11].Id.ToString("N"));

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        //next


        [Fact]
        public async Task ToEnumerableAsync_with_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = @Id", new { _userTable.Users[11].Id });

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task ToEnumerableAsync_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToEnumerableAsync<User>("id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        //list


        [Fact]
        public async Task ToListAsync_with_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("SELECT * FROM Users WHERE id = @Id", new { _userTable.Users[11].Id });

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task ToListAsync_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _uow.ToListAsync<User>("id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

    }
}
