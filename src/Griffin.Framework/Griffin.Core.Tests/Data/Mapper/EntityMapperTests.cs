using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.Tests.Data.Mapper.PropertyMappings;
using Griffin.Core.Tests.Data.Mapper.TestMappings;
using Griffin.Data.Mapper;
using Griffin.Data.Mapper.Values;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class EntityMapperTests
    {
        [Fact]
        public void map_using_field_as_setter()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new FieldSetterProperty();

            var sut = new CrudEntityMapper<FieldSetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void map_using_field_as_getter()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new FieldGetterProperty();

            var sut = new CrudEntityMapper<FieldGetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void map_using_private_property()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new PrivateProperty();

            var sut = new CrudEntityMapper<PrivateProperty>("Entities");
            sut.Map(record, actual);

            actual.GetType().GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(actual).Should().Be("1");
        }

        [Fact]
        public void map_using_private_property_setter()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new PrivateSetterProperty();

            var sut = new CrudEntityMapper<PrivateSetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void ignore_property_with_no_setter_and_no_field()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new NoSetterAndNoField();

            var sut = new CrudEntityMapper<NoSetterAndNoField>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("11");
        }

        [Fact]
        public void can_create_instance_no_specified_constructor()
        {

            var actual = CrudEntityMapper<PrivateSetterProperty>.CreateInstanceFactory()();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void can_create_instance_with_private_constructor()
        {

            var actual = CrudEntityMapper<PrivateDefaultConstructor>.CreateInstanceFactory()();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void can_create_using_the_generated_factory_method()
        {
            var sut = new CrudEntityMapper<PrivateDefaultConstructor>("Entities");
            var record = Substitute.For<IDataRecord>();

            var actual = sut.Create(record);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void throws_detailed_exception_if_no_default_constructor_exists()
        {

            Action actual = () => CrudEntityMapper<NoDefaultConstructor>.CreateInstanceFactory()();

actual.Should().Throw<MappingException>()
    .And.Message.Should().StartWith("Failed to find a default constructor ");
        }

        [Fact]
        public void get_keys()
        {
            var expected = new Ok { FirstName = "Arne", Id = "22" };

            var sut = new CrudEntityMapper<Ok>("Users");
            sut.Freeze();
            var keys = sut.GetKeys(expected);

            keys.Length.Should().Be(1);
            keys[0].Value.Should().Be(expected.Id);
        }

        [Fact]
        public void get_keys_none_is_mapped()
        {
            var expected = new Empty();

            var sut = new CrudEntityMapper<Empty>("Users");
            var keys = sut.GetKeys(expected);

            keys.Length.Should().Be(0);
        }

        [Fact]
        public void Should_be_able_to_convert_record_to_entity_with_coupled_properties()
        {
            var record = Substitute.For<IDataRecord>();
            record["Value"].Returns("1");
            record["ValueType"].Returns(1.GetType().FullName);
            var actual = new EntityWithCoupledFields();

            var sut = new EntityWithCoupledFieldsMapper();
            sut.Map(record, actual);

            actual.Value.Should().Be(1);
        }

        [Fact]
        public void only_a_setter_property()
        {
            var actual = new JustASetter();
            var record = Substitute.For<IDataRecord>();
            record["Prop"].Returns(10);

            var sut = new CrudEntityMapper<JustASetter>("Users");
            sut.Map(record, actual);

            actual.GetValue().Should().Be(10);
        }

        private class JustASetter
        {
            private int _prop;

            public int Prop { set { _prop = value; } }

            public int GetValue()
            {
                return _prop;
            }
        }
    }
}
