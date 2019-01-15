using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.Tests.Data.Mapper.TestMappings;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AssemblyScanningMappingProviderTests
    {
        [Fact]
        public void throw_on_invalid_mappers()
        {
            var sut = new AssemblyScanningMappingProvider();
            
            Action actual = () => sut.Scan(Assembly.GetExecutingAssembly());

            actual.Should().Throw<MappingException>();
        }

        [Fact]
        public void ignore_mappings_that_are_abstract()
        {
            var sut = new AssemblyScanningMappingProvider();
            sut.IgnoreInvalidMappers = true;
            sut.ReplaceDuplicateMappers = true;
            sut.Scan();

            Action actual = () => sut.Get<ClassThatHaveAnAbstractMapper>();

            actual.Should().Throw<MappingNotFoundException>();
        }

        [Fact]
        public void ignore_mappings_that_are_interfaces()
        {
            var sut = new AssemblyScanningMappingProvider();
            sut.IgnoreInvalidMappers = true;
            sut.ReplaceDuplicateMappers = true;
            sut.Scan();

            Action actual = () => sut.Get<WithInterfaceMapping>();

            actual.Should().Throw<MappingNotFoundException>();
        }

        [Fact]
        public void use_the_ok_mapping()
        {
            var sut = new AssemblyScanningMappingProvider();
            sut.IgnoreInvalidMappers = true;
            sut.ReplaceDuplicateMappers = true;
            sut.Scan();

            var actual = sut.Get<Ok>();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void protest_if_there_are_more_than_one_mapper_for_an_entity()
        {
            var sut = new AssemblyScanningMappingProvider();
            sut.IgnoreInvalidMappers = true;

            Action actual = sut.Scan;

            actual.Should().Throw<MappingException>();
        }

        [Fact]
        public void using_the_mapperfor_attribute()
        {
            var sut = new AssemblyScanningMappingProvider();
            sut.IgnoreInvalidMappers = true;
            sut.ReplaceDuplicateMappers = true;
            sut.Scan();

            var actual = sut.Get(typeof(UsingAttribute));

            actual.Should().NotBeNull();
        }
        
    }
}
