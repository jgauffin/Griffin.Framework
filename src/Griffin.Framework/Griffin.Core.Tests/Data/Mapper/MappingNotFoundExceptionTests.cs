using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class MappingNotFoundExceptionTests
    {
        [Fact]
        public void description_is_assigned_to_base()
        {
            var sut = new MappingNotFoundException(typeof(string));

            sut.Message.Should().Be("Failed to find mapper for entity 'System.String'.");
        }

        [Fact]
        public void serialization_works_with_BinaryFormatter()
        {
            var serializer = new BinaryFormatter();
            var ms = new MemoryStream();

            var sut = new MappingNotFoundException(typeof(string));
            serializer.Serialize(ms, sut);
            ms.Position = 0;
            var actual = (MappingNotFoundException)serializer.Deserialize(ms);

            actual.Message.Should().Be("Failed to find mapper for entity 'System.String'.");
            actual.EntityTypeName.Should().Be(typeof(string).FullName);
        }

        [Fact]
        public void serialization_works_with_datacontract()
        {
            var serializer = new DataContractSerializer(typeof(MappingNotFoundException));
            var ms = new MemoryStream();

            var sut = new MappingNotFoundException(typeof(string));
            serializer.WriteObject(ms, sut);
            ms.Position = 0;
            var actual = (MappingNotFoundException)serializer.ReadObject(ms);

            actual.Message.Should().Be("Failed to find mapper for entity 'System.String'.");
            actual.EntityTypeName.Should().Be(typeof(string).FullName);
        }

[Fact]
public void message_should_be_included_when_serializing_with_DataContract()
{
    var serializer = new DataContractSerializer(typeof(MappingNotFoundException));
    var ms = new MemoryStream();

    var sut = new MappingNotFoundException(typeof(string));
    serializer.WriteObject(ms, sut);
    ms.Position = 0;
    var actual = (MappingNotFoundException)serializer.ReadObject(ms);

    actual.Message.Should().Be("Failed to find mapper for entity 'System.String'.");
}

[Fact]
public void entityType_should_be_included_when_serializing_with_DataContract()
{
    var serializer = new DataContractSerializer(typeof(MappingNotFoundException));
    var ms = new MemoryStream();

    var sut = new MappingNotFoundException(typeof(string));
    serializer.WriteObject(ms, sut);
    ms.Position = 0;
    var actual = (MappingNotFoundException)serializer.ReadObject(ms);

    actual.EntityTypeName.Should().Be(typeof(string).FullName);
}


        [Fact]
        public void type_is_assigned_to_the_property()
        {
            var sut = new MappingNotFoundException(typeof(string));

            sut.EntityTypeName.Should().Be(typeof(string).FullName);
        }
    }
}