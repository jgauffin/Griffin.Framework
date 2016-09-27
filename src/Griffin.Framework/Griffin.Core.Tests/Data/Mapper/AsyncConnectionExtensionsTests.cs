using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AsyncConnectionExtensionsTests : IMappingProvider
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        [Fact]
        public async Task ToList_should_work_with_an_entity_mapper_and_a_single_line()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            var connection = Substitute.For<IDbConnection>();
            connection.CreateCommand().Returns(cmd);
            reader["Id"].Returns(10);
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());

            var actual = await connection.ToListAsync(new BasicMapper(), "SELECT * FROM Tests");

            actual[0].Id.Should().Be(10);
        }

        [Fact]
        public async Task ToList_should_work_with_an_entity_mapper_and_multiple_lines()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            var connection = Substitute.For<IDbConnection>();
            connection.CreateCommand().Returns(cmd);
            reader["Id"].Returns(10, 20);
            reader["Name"].Returns("Adam", "Bertil");
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());
            connection.CreateCommand().Returns(cmd);

            var actual = await connection.ToListAsync(new BasicMapper(), "SELECT * FROM Tests");

            actual[0].Id.Should().Be(10);
            actual[0].Name.Should().Be("Adam");
            actual[1].Id.Should().Be(20);
            actual[1].Name.Should().Be("Bertil");
        }

        public ICrudEntityMapper Get<TEntity>()
        {
            var m= new MyMapper();
            m.Freeze();
            return m;
        }

        public IEntityMapper GetBase<T>()
        {
            var m = new MyMapper();
            m.Freeze();
            return m;
        }


        public class MyTestMapper
        {
        }

        public class BasicMapper : EntityMapper<MyEntity>
        {
            
        }

        public class MyMapper : CrudEntityMapper<AsyncUnitOfWorkExtensionsTests.MyEntity>
        {
            public MyMapper()
                : base("[Table2]")
            {

                //Property(x => x.Id)
                //    .ColumnName("UserId")
                //    .ToColumnValue(propertyValue => propertyValue.ToString())
                //    .ToPropertyValue(colValue => int.Parse(colValue.ToString()))
                //    .PrimaryKey();

                //// property is considered read only.
                //Property(x => x.Name)
                //    .NotForCrud();
            }


        }
    }

}
