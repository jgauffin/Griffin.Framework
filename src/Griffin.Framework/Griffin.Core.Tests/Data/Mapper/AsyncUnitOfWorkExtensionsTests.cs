using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AsyncUnitOfWorkExtensionsTests : IMappingProvider
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
            var uow = Substitute.For<IAdoNetUnitOfWork>();
            reader["Id"].Returns(10);
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());
            uow.CreateCommand().Returns(cmd);

            var actual = await uow.ToListAsync(new BasicMapper(), "SELECT * FROM Tests");

            actual[0].Id.Should().Be(10);
        }

        [Fact]
        public async Task ToList_should_work_with_an_entity_mapper_and_multiple_lines()
        {
            var cmd = Substitute.For<DbCommand>();
            var reader = Substitute.For<DbDataReader>();
            var uow = Substitute.For<IAdoNetUnitOfWork>();
            reader["Id"].Returns(10, 20);
            reader["Name"].Returns("Adam", "Bertil");
            reader.ReadAsync().Returns(Task.FromResult(true), Task.FromResult(true), Task.FromResult(false));
            cmd.ExecuteReaderAsync().Returns(Task.FromResult(reader));
            cmd.Parameters.Returns(Substitute.For<DbParameterCollection>());
            uow.CreateCommand().Returns(cmd);

            var actual = await uow.ToListAsync(new BasicMapper(), "SELECT * FROM Tests");

            actual[0].Id.Should().Be(10);
            actual[0].Name.Should().Be("Adam");
            actual[1].Id.Should().Be(20);
            actual[1].Name.Should().Be("Bertil");
        }

        [Fact]
        public async Task Should_be_able_to_delete_an_entity()
        {
            var cmd = Substitute.For<DbCommand>();
            var uow = Substitute.For<IAdoNetUnitOfWork>();
            var myEntity = new MyEntity();
            var ps = Substitute.For<DbParameterCollection>();
            cmd.Parameters.Returns(ps);
            cmd.CreateParameter().Returns(Substitute.For<DbParameter>());
            uow.CreateCommand().Returns(cmd);

            await uow.DeleteAsync(new MyMapper(), myEntity);

            await cmd.Received().ExecuteNonQueryAsync();
            cmd.CommandText.Should().Be("DELETE FROM [Table2] WHERE Id=@Id");
        }

        [Fact]
        public void Should_abort_if_there_is_no_primary_key()
        {
            var cmd = Substitute.For<DbCommand>();
            var uow = Substitute.For<IAdoNetUnitOfWork>();
            var myEntity = new MyEntity2();
            var ps = Substitute.For<DbParameterCollection>();
            cmd.Parameters.Returns(ps);
            cmd.CreateParameter().Returns(Substitute.For<DbParameter>());
            uow.CreateCommand().Returns(cmd);

            var actual = ()=> uow.DeleteAsync(new MyMapper2(), myEntity);

            actual.Should().ThrowAsync<MappingException>();
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

        public class MyEntity2
        {
            public int DomainId { get; set; }
        }
        public class BasicMapper : EntityMapper<MyEntity>
        {
            
        }
        public class MyMapper2 : CrudEntityMapper<MyEntity2>
        {
            public MyMapper2()
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
