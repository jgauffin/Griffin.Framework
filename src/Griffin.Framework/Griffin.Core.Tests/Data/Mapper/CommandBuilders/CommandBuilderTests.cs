using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.CommandBuilders;
using Griffin.Data.Mapper.Values;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper.CommandBuilders
{
    public class CommandBuilderTests
    {
        [Fact]
        public void Mapper_is_required_for_the_class_to_work()
        {

            Action actual = () => new CommandBuilder(null);

            actual.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TableName_is_properly_assigned_in_the_constructor()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");

            var sut = new CommandBuilder(mapper);

            sut.TableName.Should().Be("Users");
        }

        [Fact]
        public void key_is_added_in_the_insert_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello",IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new {Id = "Hello"};


            var sut = new CommandBuilder(mapper);
            sut.InsertCommand(command, entity);

            command.CommandText.Should().Be("INSERT INTO Users (id) VALUES(@Id)");
            command.Parameters[0].ParameterName.Should().Be("Id");
            command.Parameters[0].Value.Should().Be("Hello");
        }

        [Fact]
        public void key_without_value_is_ignored_in_the_insert_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual =  () => sut.InsertCommand(command, entity);

            actual.Should().Throw<DataException>();
        }

        [Fact]
        public void field_is_added_in_the_insert_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", CanRead = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.InsertCommand(command, entity);

            command.CommandText.Should().Be("INSERT INTO Users (id) VALUES(@Id)");
            command.Parameters[0].ParameterName.Should().Be("Id");
            command.Parameters[0].Value.Should().Be("Hello");
        }

        [Fact]
        public void field_without_value_is_added_with_DbNull_in_the_insert_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id") {CanRead = true} }
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.InsertCommand(command, entity);

            command.CommandText.Should().Be("INSERT INTO Users (id) VALUES(@Id)");
            command.Parameters[0].ParameterName.Should().Be("Id");
            command.Parameters[0].Value.Should().Be(DBNull.Value);
        }

        [Fact]
        public void key_is_added_first_in_the_insert_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello",IsPrimaryKey = true}},
                {"Name", new FakePropertyMapping("Name", "name"){Value = "Arne", CanRead = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.InsertCommand(command, entity);

            command.CommandText.Should().Be("INSERT INTO Users (id, name) VALUES(@Id, @Name)");
            command.Parameters[0].ParameterName.Should().Be("Id");
            command.Parameters[0].Value.Should().Be("Hello");
            command.Parameters[1].ParameterName.Should().Be("Name");
            command.Parameters[1].Value.Should().Be("Arne");
        }



        [Fact]
        public void keys_must_be_specified_in_an_update_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual = () => sut.UpdateCommand(command, entity);

            actual.Should().Throw<DataException>();
        }

        [Fact]
        public void key_may_not_be_the_only_field_in_an_update_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual = () => sut.UpdateCommand(command, entity);

            actual.Should().Throw<DataException>();
        }

        [Fact]
        public void field_is_added_in_the_update_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name"){Value = "Arne", CanRead = true}},
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.UpdateCommand(command, entity);

            command.CommandText.Should().Be("UPDATE Users SET real_name=@Name WHERE id=@Id");
            command.Parameters[0].ParameterName.Should().Be("Name");
            command.Parameters[0].Value.Should().Be("Arne");
            command.Parameters[1].ParameterName.Should().Be("Id");
            command.Parameters[1].Value.Should().Be("Hello");
        }

        [Fact]
        public void field_without_value_is_added_with_DbNull_in_the_update_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name") {CanRead = true} },
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.UpdateCommand(command, entity);

            command.CommandText.Should().Be("UPDATE Users SET real_name=@Name WHERE id=@Id");
            command.Parameters[0].ParameterName.Should().Be("Name");
            command.Parameters[0].Value.Should().Be(DBNull.Value);
            command.Parameters[1].ParameterName.Should().Be("Id");
            command.Parameters[1].Value.Should().Be("Hello");
        }


        [Fact]
        public void key_must_a_value_in_the_update_Query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name"){Value = "Hello" }},
                {"Id", new FakePropertyMapping("Id", "id"){IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual = () => sut.UpdateCommand(command, entity);

            actual.Should().Throw<DataException>();
        }


        [Fact]
        public void keys_must_be_specified_in_a_delete_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Id", new FakePropertyMapping("Id", "id"){IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual = () => sut.DeleteCommand(command, entity);

            actual.Should().Throw<DataException>();
        }

        [Fact]
        public void key_should_be_the_only_field_in_a_delete_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name"){Value = "Arne"}},
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.DeleteCommand(command, entity);

            command.Parameters.Count.Should().Be(1);
        }

        [Fact]
        public void key_is_assigned_correctly_in_a_delete_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name"){Value = "Arne"}},
                {"Id", new FakePropertyMapping("Id", "id"){Value = "Hello", IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            sut.DeleteCommand(command, entity);

            command.CommandText.Should().Be("DELETE FROM Users WHERE id=@Id");
            command.Parameters[0].ParameterName.Should().Be("Id");
            command.Parameters[0].Value.Should().Be("Hello");
        }

        [Fact]
        public void do_not_allow_DbNull_in_the_delete_query()
        {
            var mapper = Substitute.For<ICrudEntityMapper>();
            mapper.TableName.Returns("Users");
            mapper.Properties.Returns(new Dictionary<string, IPropertyMapping>
            {
                {"Name", new FakePropertyMapping("Name", "real_name")},
                {"Id", new FakePropertyMapping("Id", "id"){Value = DBNull.Value, IsPrimaryKey = true}}
            });
            var command = new AdoNetFakes.FakeCommand();
            var entity = new { Id = "Hello" };


            var sut = new CommandBuilder(mapper);
            Action actual = () => sut.DeleteCommand(command, entity);

            actual.Should().Throw<DataException>();
        }


    }
}
