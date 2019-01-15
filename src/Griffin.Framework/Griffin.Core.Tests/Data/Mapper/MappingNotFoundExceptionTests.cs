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
        public void type_is_assigned_to_the_property()
        {
            var sut = new MappingNotFoundException(typeof(string));

            sut.EntityTypeName.Should().Be(typeof(string).FullName);
        }
    }
}