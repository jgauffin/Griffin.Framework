using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void try_conversion_to_unix()
        {

            var sut = DateTime.UtcNow;
            var actual = sut.ToUnixTime();

            actual.Should().Be(sut.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        [Fact]
        public void conversion_from_int_unix()
        {
            var expected = DateTime.UtcNow.TruncateMilliseconds();
            var unix = (int) expected.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var actual = unix.FromUnixTime();

            actual.Should().Be(expected);
        }

        [Fact]
        public void conversion_from_double_unix()
        {
            var expected = DateTime.UtcNow.TruncateMilliseconds();
            var unix = expected.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var actual = unix.FromUnixTime();

            actual.Should().Be(expected);
        }
    }
}
