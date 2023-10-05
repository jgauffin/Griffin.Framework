using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using FluentAssertions;
using Griffin.AdoNetFakes;
using Griffin.Core.Tests.Data.Mapper.TestMappings;
using Griffin.Data;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class CommandExtensionsTests
    {
        [Fact]
        public void ApplyConstraints_should_work_with_a_int_if_there_is_a_primary_key()
        {
            var mapper = new OkMapping();
            var cmd = new SqlCommand();

            cmd.ApplyConstraints(mapper, 1);

            cmd.Parameters[0].As<DbParameter>().Value.Should().Be(1);
            cmd.Parameters[0].As<DbParameter>().ParameterName.Should().Be("Id");
        }

        [Fact]
        public void ApplyConstraints_should_work_with_a_string_if_there_is_a_primary_key()
        {
            var mapper = new OkMapping();
            var cmd = new SqlCommand();

            cmd.ApplyConstraints(mapper,"hey");

            cmd.Parameters[0].As<DbParameter>().Value.Should().Be("hey");
            cmd.Parameters[0].As<DbParameter>().ParameterName.Should().Be("Id");
        }

        [Fact]
        public void ApplyConstraints_should_work_with_a_dynamicObject_if_there_is_a_primary_key()
        {
            var mapper = new OkMapping();
            var cmd = new SqlCommand();

            cmd.ApplyConstraints(mapper, new{firstName="arne"});

            cmd.Parameters[0].As<DbParameter>().Value.Should().Be("arne");
            cmd.Parameters[0].As<DbParameter>().ParameterName.Should().Be("firstName");
        }


        [Fact]
        public void Command_without_rows_throw_exception_for_First()
        {
            var cmd = Substitute.For<IDbCommand>();

            Action actual = () => cmd.First<string>();

            actual.Should().Throw<EntityNotFoundException>();
        }

        [Fact]
        public void Command_without_rows_throw_exception_for_First_with_specified_mapper()
        {
            var cmd = Substitute.For<IDbCommand>();

            Action actual = () => cmd.First(new TentityMapper());

            actual.Should().Throw<EntityNotFoundException>();
        }

        [Fact]
        public void Command_with_rows_should_return_first_row_with_First()
        {
            var table = new FakeTable(new[] {new object[] {"10"}}, new [] { "Id" });
            var cmd = new FakeCommand(table);
            cmd.CommandText = "MustBespecifiedForFakeCmd";

            var actual = cmd.First(new TentityMapper());

            actual.Id.Should().Be("10");
        }

        [Fact]
        public void Command_with_rows_should_return_first_row_with_FirstOrDefault()
        {
            var table = new FakeTable(new[] { new object[] { "10" } }, new[] { "Id" });
            var cmd = new FakeCommand(table);
            cmd.CommandText = "MustBespecifiedForFakeCmd";

            var actual = cmd.FirstOrDefault(new TentityMapper());

            actual.Id.Should().Be("10");
        }

        [Fact]
        public void Command_without_rows_should_return_null_for_FirstOrDefault()
        {
            var cmd = Substitute.For<IDbCommand>();

            var actual = cmd.FirstOrDefault(new TentityMapper());

            actual.Should().BeNull();
        }

        [Fact]
        public void ToEnumerable_without_arguments_generates_an_enumerable_without_connection_ownership()
        {
            var provider = Substitute.For<IMappingProvider>();
            provider.GetBase<Tentity>().Returns(new TentityMapper());
            EntityMappingProvider.Provider = provider;
            var connection = new FakeConnection(){CurrentState = ConnectionState.Open};
            var cmd = new FakeCommand(connection, new CommandResult[]{new ReaderCommandResult(){Result = new FakeDataReader(new DataTable())}, });
            cmd.CommandText = "Hello";

            var actual = cmd.ToEnumerable<Tentity>();
            actual.GetEnumerator().Dispose();

            connection.State.Should().Be(ConnectionState.Open);
        }

        [Fact]
        public void ToList_should_fill_the_list()
        {
            var connection = new FakeConnection() {CurrentState = ConnectionState.Open};
            var table = new FakeTable(new object[][] {new object[] {1},}, new String[] { "Id" });
            var result = new CommandResult[] {new ReaderCommandResult {Result = new FakeDataReader(table)}};
            var cmd = new FakeCommand(connection, result);
            cmd.CommandText = "Hello";

            var actual = cmd.ToList<Tentity>(new TentityMapper());

            actual[0].Id.Should().Be("1");
        }

        public class Tentity
        {
            public string Id { get; set; }
        }

        public class TentityMapper : CrudEntityMapper<Tentity>
        {
            public TentityMapper() : base("Tenteties")
            {
            }
        }

    }


}
