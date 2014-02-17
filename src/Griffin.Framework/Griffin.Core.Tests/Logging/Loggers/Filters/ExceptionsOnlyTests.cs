using System;
using FluentAssertions;
using Griffin.Logging;
using Griffin.Logging.Loggers.Filters;
using Xunit;

namespace Griffin.Core.Tests.Logging.Loggers.Filters
{
    public class ExceptionsOnlyTests
    {
        [Fact]
        public void reject_entry_without_exception()
        {
            var sut = new ExceptionsOnly();
            var entry = new LogEntry(LogLevel.Trace, "kkjlsdfkl", null);

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeFalse();
        }

        [Fact]
        public void accept_entries_that_got_an_exception()
        {
            var sut = new ExceptionsOnly();
            var entry = new LogEntry(LogLevel.Trace, "kkjlsdfkl", new Exception());

            var actual = sut.IsSatisfiedBy(entry);

            actual.Should().BeTrue();
        }
    }
}
