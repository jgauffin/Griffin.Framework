using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AdoNetEntityEnumerableTests
    {
        [Fact]
        public void should_not_be_able_to_call_enumerator_twice_as_the_underlying_reader_do_not_allow_that()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerable<string>(cmd, reader, mapper, false);
            sut.GetEnumerator();
            Action actual = () => sut.GetEnumerator();

            actual.Should().Throw<InvalidOperationException>();
        }

    }
}
