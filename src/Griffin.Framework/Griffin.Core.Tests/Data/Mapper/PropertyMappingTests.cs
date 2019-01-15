using System;
using System.Data;
using FluentAssertions;
using Griffin.Core.Tests.Data.Mapper.TestMappings;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.Values;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class PropertyMappingTests
    {
        [Fact]
        public void constructor_assigns_property_name_to_property()
        {

            var sut = new PropertyMapping<Ok>("Abra", (o, o1) => { }, o => o);

            sut.PropertyName.Should().Be("Abra");
            sut.ColumnName.Should().Be("Abra");
        }

        [Fact]
        public void map_without_adapter()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();
            record["Id"].Returns("Hej");

            var sut = new PropertyMapping<Ok>("Id", (o, o1) => o.Id = (string)o1, o => o.Id);
            sut.Map(record, actual);

            actual.Id.Should().Be("Hej");
        }

        [Fact]
        public void use_the_adapter_son()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();
            record["Id"].Returns("Hej");

            var sut = new PropertyMapping<Ok>("Id", (o, o1) => o.Id = (string)o1, ok => ok.Id);
            sut.ColumnToPropertyAdapter = o => o + "Hello";
            sut.Map(record, actual);

            actual.Id.Should().Be("HejHello");
        }

        [Fact]
        public void should_replace_dbnull_with_the_defined_replacement_value_when_mapping_column_value()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();
            record["Age"].Returns(DBNull.Value);

            var sut = new PropertyMapping<Ok>("Age", (o, o1) => o.Age= (int)o1, ok => ok.Age);
            sut.NullValue = 1;
            sut.Map(record, actual);

            actual.Age.Should().Be(1);
        }

        [Fact]
        public void should_be_able_to_convert_null_value_to_dbnull()
        {
            var record = Substitute.For<IDataRecord>();
            var entity = new Ok() {Age = 1};

            var sut = (IPropertyMapping)new PropertyMapping<Ok>("Age", (o, o1) => o.Age = (int)o1, ok => ok.Age);
            sut.NullValue = 1;
            var actual = sut.GetValue(entity);

            actual.Should().Be(DBNull.Value);
        }

        [Fact]
        public void use_column_name_and_not_property_name_in_the_record()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();
            record["Id2"].Returns("Hej");

            var sut = new PropertyMapping<Ok>("Id", (o, o1) => o.Id = (string)o1, ok => ok.Id);
            sut.ColumnName = "Id2";
            sut.Map(record, actual);

            actual.Id.Should().Be("Hej");
        }

        [Fact]
        public void skip_db_null_value()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();
            record["Id2"].Returns(DBNull.Value);

            var sut = new PropertyMapping<Ok>("Id", (o, o1) => o.Id = (string)o1, ok => ok.Id);
            sut.ColumnName = "Id2";
            sut.Map(record, actual);

            actual.Id.Should().BeNull();
        }

        [Fact]
        public void set_Value_db_null()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();

            var sut = new PropertyMapping<Ok>("FirstName", (o, o1) => o.FirstName = (string)o1, ok => ok.FirstName);
            sut.SetProperty(actual, DBNull.Value);

            actual.FirstName.Should().Be(null);
        }

        [Fact]
        public void set_Value_something()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();

            var sut = new PropertyMapping<Ok>("FirstName", (o, o1) => o.FirstName = (string)o1, ok => ok.FirstName);
            sut.PropertyType = typeof (string);
            sut.SetProperty(actual, "Hello");

            actual.FirstName.Should().Be("Hello");
        }

        [Fact]
        public void convert_if_Required()
        {
            var record = Substitute.For<IDataRecord>();
            var actual = new Ok();

            var sut = new PropertyMapping<Ok>("Age", (o, o1) => o.Age = (int)o1, ok => ok.Age);
            sut.PropertyType = typeof(int);
            sut.SetProperty(actual, (decimal)1);

            actual.Age.Should().Be(1);
        }
    }
}
