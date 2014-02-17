using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class EntityMappingProviderTests
    {
        [Fact]
        public void must_specify_a_new_provider()
        {

            Action actual = () => EntityMappingProvider.Provider = null;

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void the_new_provider_is_used_after_assignment()
        {
            var provider = Substitute.For<IMappingProvider>();
            var expected = Substitute.For<IEntityMapper<string>>();
            provider.Get<string>().Returns(expected);

            EntityMappingProvider.Provider = provider;
            var actual = EntityMappingProvider.GetMapper<string>();


            actual.Should().Be(expected);
        }
    }
}
