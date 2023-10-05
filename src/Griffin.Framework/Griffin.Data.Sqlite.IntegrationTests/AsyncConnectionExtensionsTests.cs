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
    public class AsyncConnectionExtensionsTests
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly UserTable _userTable = new UserTable();

        public AsyncConnectionExtensionsTests()
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
        public async Task FirstOrDefault_with_objectConstraint_should_return_wanted_row()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>(new { _userTable.Users[11].FirstName });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = @Id", new{ _userTable.Users[11].Id });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task FirstOrDefault_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            actual.Should().NotBeNull();
            actual.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task FirstOrDefault_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.FirstOrDefaultAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

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

            var actual = await _connection.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToEnumerableAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = @Id", new { _userTable.Users[11].Id });

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToEnumerableAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task ToEnumerableAsync_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToEnumerableAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToEnumerableAsync_with_short_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToEnumerableAsync<User>("id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual.First();
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        //list


        [Fact]
        public async Task ToListAsync_with_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("SELECT * FROM Users WHERE id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_no_parameters()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("id = '" + _userTable.Users[11].Id.ToString("N") + "'");

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("SELECT * FROM Users WHERE id = @Id", new { _userTable.Users[11].Id });

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_anon_parameter()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("id = @Id", new { _userTable.Users[11].Id });

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task ToListAsync_with_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("SELECT * FROM Users WHERE id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }

        [Fact]
        public async Task ToListAsync_with_short_query_and_value_array()
        {
            _userTable.Insert(_connection, 50);

            var actual = await _connection.ToListAsync<User>("id = @1", _userTable.Users[11].Id.ToString("N"));

            var first = actual[0];
            first.LastName.Should().Be(_userTable.Users[11].LastName);
        }


        [Fact]
        public async Task Insert_row_with_auto_increment()
        {
            var table = new SimpleUserTable();
            table.Create(_connection);
            var expected = new SimpleUser() { FirstName = "Arne"};

            await _connection.InsertAsync(expected);

            var actual = await _connection.FirstAsync<SimpleUser>(new { expected.Id });
            actual.FirstName.Should().Be(expected.FirstName);
            actual.Id.Should().Be(expected.Id);
        }



        [Fact]
        public async Task First_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[10];

            var actual = await _connection.FirstAsync<User>(new { _userTable.Users[10].Id });

            actual.LastName.Should().Be(expected.LastName);
        }




        [Fact]
        public async Task Delete_user()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _connection.DeleteAsync(expected);

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }

        [Fact]
        public async Task Delete_user_using_constraints()
        {
            _userTable.Insert(_connection, 50);
            var expected = _userTable.Users[0];

            await _connection.DeleteAsync<User>(new { expected.Id });

            var actual = await _connection.FirstOrDefaultAsync<User>(new { expected.Id });
            actual.Should().BeNull();
        }


        [Fact]
        public async Task Update_user()
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
