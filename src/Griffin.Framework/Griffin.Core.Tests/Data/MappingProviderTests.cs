using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data
{
    public class EntityMappingProviderTests
    {
        [Fact]
        public void may_only_set_one_mapping_provider()
        {
            var provider = Substitute.For<IMappingProvider>();

            EntityMappingProvider.Provider = provider;

        }
    }
}
