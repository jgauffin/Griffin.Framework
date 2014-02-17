using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class MappingForAttributeTests
    {
        [Fact]
        public void type_should_Be_assigned_to_property()
        {

            var sut = new MappingForAttribute(typeof(string));

            sut.EntityType.Should().Be(typeof (string));
        }
    }
}
