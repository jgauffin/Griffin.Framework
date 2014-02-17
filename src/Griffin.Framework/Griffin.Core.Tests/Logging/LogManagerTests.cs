using System;
using FluentAssertions;
using Griffin.Logging;
using Griffin.Logging.Loggers;
using NCrunch.Framework;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Logging
{
    public class LogManagerTests
    {
        [Fact, ExclusivelyUses("LogManager")]
        public void default_setting_returns_null_logger_to_get_rid_of_null_checks()
        {

            var actual = LogManager.GetLogger<LogManagerTests>();

            actual.Should().BeOfType<NullLogger>();
        }

        [Fact, ExclusivelyUses("LogManager")]
        public void assigning_a_new_provider_removes_the_old_one()
        {
            var provider = Substitute.For<ILogProvider>();
            var expected = Substitute.For<ILogger>();
            provider.GetLogger(Arg.Any<Type>()).Returns(expected);

            var actual1 = LogManager.GetLogger<LogManagerTests>();
            LogManager.Provider = provider;
            var actual2 = LogManager.GetLogger<LogManagerTests>();

            actual1.Should().BeOfType<NullLogger>();
            actual2.Should().Be(expected);
        }

        [Fact, ExclusivelyUses("LogManager")]
        public void get_logger_using_the_non_generic_method_should_return_expected_logger()
        {
            var provider = Substitute.For<ILogProvider>();
            var expected = Substitute.For<ILogger>();
            provider.GetLogger(Arg.Any<Type>()).Returns(expected);

            LogManager.Provider = provider;
            var actual = LogManager.GetLogger(GetType());

            actual.Should().Be(expected);
        }

    }
}
