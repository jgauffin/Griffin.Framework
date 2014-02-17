using FluentAssertions;
using Griffin.Logging;
using Griffin.Logging.Loggers.Filters;
using Xunit;

namespace Griffin.Core.Tests.Logging.Loggers.Filters
{
    public class LogLevelFilterTests
    {
        [Fact]
        public void min_default_level_is_debug()
        {
            var sut = new LogLevelFilter();

            sut.MinLevel.Should().Be(LogLevel.Debug);
        }

        [Fact]
        public void max_default_level_is_error()
        {
            var sut = new LogLevelFilter();

            sut.MinLevel.Should().Be(LogLevel.Error);
        }

        [Fact]
        public void lower_level_rejects_those_under()
        {
            var sut = new LogLevelFilter();
            sut.MinLevel = LogLevel.Warning;
            var entry = new LogEntry(LogLevel.Info, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeFalse();
        }

        [Fact]
        public void lower_level_is_inclusive()
        {
            var sut = new LogLevelFilter();
            sut.MinLevel = LogLevel.Warning;
            var entry = new LogEntry(LogLevel.Warning, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeTrue();
        }

        [Fact]
        public void lower_level_is_accepting_above_ones()
        {
            var sut = new LogLevelFilter();
            sut.MinLevel = LogLevel.Info;
            var entry = new LogEntry(LogLevel.Warning, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeTrue();
        }

        [Fact]
        public void higher_level_rejects_those_above()
        {
            var sut = new LogLevelFilter();
            sut.MaxLevel = LogLevel.Info;
            var entry = new LogEntry(LogLevel.Warning, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeFalse();
        }

        [Fact]
        public void higher_level_is_inclusive()
        {
            var sut = new LogLevelFilter();
            sut.MaxLevel = LogLevel.Warning;
            var entry = new LogEntry(LogLevel.Warning, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeTrue();
        }

        [Fact]
        public void higher_level_is_accepting_below_ones()
        {
            var sut = new LogLevelFilter();
            sut.MaxLevel = LogLevel.Info;
            var entry = new LogEntry(LogLevel.Debug, "kksdl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeTrue();
        }
    }
}