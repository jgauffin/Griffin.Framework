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

            var sut = new EntityMapper<FieldSetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void map_using_field_as_getter()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new FieldGetterProperty();

            var sut = new EntityMapper<FieldGetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void map_using_private_property()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new PrivateProperty();

            var sut = new EntityMapper<PrivateProperty>("Entities");
            sut.Map(record, actual);

            actual.GetType().GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(actual).Should().Be("1");
        }

        [Fact]
        public void map_using_private_property_setter()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new PrivateSetterProperty();

            var sut = new EntityMapper<PrivateSetterProperty>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("1");
        }

        [Fact]
        public void ignore_property_with_no_setter_and_no_field()
        {
            var record = Substitute.For<IDataRecord>();
            record["Id"].Returns("1");
            var actual = new NoSetterAndNoField();

            var sut = new EntityMapper<NoSetterAndNoField>("Entities");
            sut.Map(record, actual);

            actual.Id.Should().Be("11");
        }

        [Fact]
        public void can_create_instance_no_specified_constructor()
        {

            var actual = EntityMapper<PrivateSetterProperty>.CreateInstanceFactory()();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void can_create_instance_with_private_constructor()
        {

            var actual = EntityMapper<PrivateDefaultConstructor>.CreateInstanceFactory()();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void can_create_using_the_generated_factory_method()
        {
            var sut = new EntityMapper<PrivateDefaultConstructor>("Entities");
            var record = Substitute.For<IDataRecord>();

            var actual = sut.Create(record);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void throws_detailed_exception_if_no_default_constructor_exists()
        {

            Action actual = () => EntityMapper<NoDefaultConstructor>.CreateInstanceFactory()();

            actual.ShouldThrow<MappingException>().And.Message.Should().StartWith("Failed to find a default constructor ");
        }

        [Fact]
        public void get_keys()
        {
            var expected = new Ok { FirstName = "Arne", Id = "22" };

            var sut = new EntityMapper<Ok>("Users");
            sut.Freeze();
            var keys = sut.GetKeys(expected);

            keys.Length.Should().Be(1);
            keys[0].Value.Should().Be(expected.Id);
        }

        [Fact]
        public void get_keys_none_is_mapped()
        {
            var expected = new Empty();

            var sut = new EntityMapper<Empty>("Users");
            var keys = sut.GetKeys(expected);

            keys.Length.Should().Be(0);
        }

        [Fact]
        public void only_a_setter_property()
        {
            var actual = new JustASetter();
            var record = Substitute.For<IDataRecord>();
            record["Prop"].Returns(10);

            var sut = new EntityMapper<JustASetter>("Users");
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
