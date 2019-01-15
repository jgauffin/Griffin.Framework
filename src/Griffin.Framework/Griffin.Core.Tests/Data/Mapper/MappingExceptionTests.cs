using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FluentAssertions;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class MappingExceptionTests
    {
        [Fact]
        public void type_is_assigned_to_the_property()
        {
            
            var sut = new MappingException(typeof(string), "Hello");

            sut.EntityTypeName.Should().Be(typeof (string).FullName);
        }

        [Fact]
        public void description_is_assigned_to_base()
        {

            var sut = new MappingException(typeof(string), "Hello");

            sut.Message.Should().Be("Hello");
        }
    }
}
