using FluentAssertions;
using Griffin.Logging;
using Griffin.Logging.Loggers.Filters;
using Xunit;

namespace Griffin.Core.Tests.Logging.Loggers.Filters
{
    public class NoFilterTests
    {
        [Fact]
        public void should_always_accept_everything_honey()
        {
            var sut = NoFilter.Instance;

            var actual = sut.IsSatisfiedBy(new LogEntry(LogLevel.Trace, "kjjklsdf", null));

            actual.Should().BeTrue();
        }
    }
}
