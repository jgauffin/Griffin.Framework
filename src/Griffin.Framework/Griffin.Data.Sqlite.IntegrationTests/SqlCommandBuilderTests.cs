using System;
using System.Data.SQLite;
using System.IO;
using FluentAssertions;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Sqlite.IntegrationTests.Entites;
using Xunit;

namespace Griffin.Data.Sqlite.IntegrationTests
{
    public class SqlCommandBuilderTests : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbFile;
        private readonly NewsTable _newsTable = new NewsTable();

        public SqlCommandBuilderTests()
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));

            _dbFile = Path.GetTempFileName();
            var cs = "URI=file:" + _dbFile;
            _connection = new SQLiteConnection(cs);
            _connection.Open();

            _newsTable.Create(_connection);
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
        public void should_assign_id_to_the_created_entity_when_mapping_says_so()
        {
            var newsItem = new News {Title = "Major Headline"};

            _connection.Insert(newsItem);

            newsItem.Id.Should().Be(1);
        }

        [Fact]
        public void paging_selects_correct_page()
        {
            _newsTable.Insert(_connection, 40);

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM News";
                cmd.Paging<News>(5, 5);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    var id = reader.GetInt32(0);
                    id.Should().Be(21);
                }
            }
        }

        [Fact]
        public void direct_paging_selects_correct_page()
        {
            _newsTable.Insert(_connection, 40);

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM News";
                cmd.Paging(5, 5);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    var id = reader.GetInt32(0);
                    id.Should().Be(21);
                }
            }
        }
    }
}