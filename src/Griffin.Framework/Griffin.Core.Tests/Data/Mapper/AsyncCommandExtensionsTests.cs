using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.AdoNetFakes;
using Griffin.Data;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AsyncCommandExtensionsTests
    {
        [Fact]
        public void command_without_rows_throw_exception_for_First()
        {
            var cmd = Substitute.For<DbCommand>();
            var task = Task.Factory.StartNew(() => Substitute.For<DbDataReader>());
            cmd.ExecuteReaderAsync().Returns(task);
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            Action actual = () => cmd.FirstAsync(new TentityMapper()).Wait();

            actual.ShouldThrow<EntityNotFoundException>();
        }

        [Fact]
        public async Task command_with_rows_should_return_first_row_with_First()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            reader["Id"].Returns("10");
            reader.ReadAsync().Returns(Task.FromResult(true));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            var actual = await cmd.FirstAsync(new TentityMapper());

            actual.Id.Should().Be("10");
        }

        [Fact]
        public async Task command_with_rows_should_return_first_row_with_FirstOrDefault()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            reader["Id"].Returns("10");
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            await cmd.FirstOrDefaultAsync(new TentityMapper());

            reader.ReceivedCalls().Count(x => x.GetMethodInfo().Name == "ReadAsync").Should().Be(1);
        }

        [Fact]
        public async Task command_without_rows_should_return_null_for_FirstOrDefault()
        {
            var cmd = Substitute.For<DbCommand>();
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(Substitute.For<DbDataReader>()));

            var actual = await cmd.FirstOrDefaultAsync(new TentityMapper());

            actual.Should().BeNull();
        }

        [Fact]
        public async Task ToEnumerable_without_arguments_generates_an_enumerable_without_connection_ownership()
        {
            var provider = Substitute.For<IMappingProvider>();
            provider.Get<Tentity>().Returns(new TentityMapper());
            EntityMappingProvider.Provider = provider;
            var cmd = Substitute.For<DbCommand>();
            var connection = Substitute.For<DbConnection>();
            var reader = Substitute.For<DbDataReader>();
            cmd.Connection.Returns(connection);
            reader["Id"].Returns("10");
            reader.ReadAsync().Returns(Task.FromResult(true));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            var actual = await cmd.ToEnumerableAsync<Tentity>();
            actual.GetEnumerator().Dispose();

            connection.DidNotReceive().Close();
            connection.DidNotReceive().Dispose();
        }

        [Fact]
        public async Task ToEnumerable_with_ownership_should_dispose_connection()
        {
            var provider = Substitute.For<IMappingProvider>();
            provider.Get<Tentity>().Returns(new TentityMapper());
            EntityMappingProvider.Provider = provider;
            var cmd = Substitute.For<DbCommand>();
            var connection = Substitute.For<DbConnection>();
            var reader = Substitute.For<DbDataReader>();
            cmd.Connection.Returns(connection);
            reader["Id"].Returns("10");
            reader.ReadAsync().Returns(Task.FromResult(true));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            var actual = await cmd.ToEnumerableAsync<Tentity>(true);
            actual.GetEnumerator().Dispose();

            connection.Received().Dispose();
        }

        [Fact]
        public async Task  ToList_should_fill_the_list()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            reader["Id"].Returns("10");
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            var actual = await cmd.ToListAsync<Tentity>(new TentityMapper());

            actual[0].Id.Should().Be("10");
        }

        public class Tentity
        {
            public string Id { get; set; }
        }

        public class TentityMapper : EntityMapper<Tentity>
        {
            public TentityMapper() : base("Tenteties")
            {
            }
        }

    }


}
