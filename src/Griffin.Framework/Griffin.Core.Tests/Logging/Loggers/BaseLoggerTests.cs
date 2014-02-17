using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Griffin.Logging;
using Xunit;

namespace Griffin.Core.Tests.Logging.Loggers
{
    public class BaseLoggerTests
    {
        [Fact]
        public void trace_without_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Trace("Heloo");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Trace);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void trace_formatted()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Trace("Heloo {0}", "World");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo World");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Trace);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void trace_with_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Trace("Heloo", new ExternalException());

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Trace);
            sut.Entries[0].Exception.Should().BeOfType<ExternalException>();
        }


        [Fact]
        public void debug_without_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Debug("Heloo");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Debug);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void debug_formatted()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Debug("Heloo {0}", "World");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo World");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Debug);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void debug_with_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Debug("Heloo", new ExternalException());

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Debug);
            sut.Entries[0].Exception.Should().BeOfType<ExternalException>();
        }

        [Fact]
        public void info_without_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Info("Heloo");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Info);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void info_formatted()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Info("Heloo {0}", "World");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo World");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Info);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void info_with_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Info("Heloo", new ExternalException());

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Info);
            sut.Entries[0].Exception.Should().BeOfType<ExternalException>();
        }

        [Fact]
        public void warning_without_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Warning("Heloo");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Warning);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void warning_formatted()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Warning("Heloo {0}", "World");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo World");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Warning);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void warning_with_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Warning("Heloo", new ExternalException());

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Warning);
            sut.Entries[0].Exception.Should().BeOfType<ExternalException>();
        }

        [Fact]
        public void Error_without_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Error("Heloo");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Error);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void Error_formatted()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Error("Heloo {0}", "World");

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo World");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Error);
            sut.Entries[0].Exception.Should().BeNull();
        }

        [Fact]
        public void Error_with_exception()
        {

            var sut = new BaseLoggerWrapper(GetType());
            sut.Error("Heloo", new ExternalException());

            sut.Entries.Count.Should().Be(1);
            sut.Entries[0].Message.Should().Be("Heloo");
            sut.Entries[0].LogLevel.Should().Be(LogLevel.Error);
            sut.Entries[0].Exception.Should().BeOfType<ExternalException>();
        }

        [Fact]
        public void format_a_simple_exception()
        {
            var exception = new NotImplementedException();

            var sut = new BaseLoggerWrapper(GetType());
            var actual = sut.FormatException(exception);

            actual.Should().Be("    System.NotImplementedException: The method or operation is not implemented.\r\n");
        }

        [Fact]
        public void include_property_in_the_exception_output()
        {
            var exception = new ExceptionWithProperty(){UserId = 10};

            var sut = new BaseLoggerWrapper(GetType());
            var actual = sut.FormatException(exception);

            actual.Should().Be("    Griffin.Tests.Logging.Loggers.ExceptionWithProperty: Exception of type 'Griffin.Tests.Logging.Loggers.ExceptionWithProperty' was thrown.\r\n    [UserId='10']\r\n");
        }

        [Fact]
        public void include_properties_in_the_exception_output()
        {
            var exception = new ExceptionWithProperty2() { UserId = 10, FirstName = "Arne"};

            var sut = new BaseLoggerWrapper(GetType());
            var actual = sut.FormatException(exception);

            actual.Should().Be("    Griffin.Tests.Logging.Loggers.ExceptionWithProperty2: Exception of type 'Griffin.Tests.Logging.Loggers.ExceptionWithProperty2' was thrown.\r\n    [UserId='10',FirstName='Arne']\r\n");
        }

        [Fact]
        public void ignore_null_properties_in_the_exception_output()
        {
            var exception = new ExceptionWithProperty2() { UserId = 10};

            var sut = new BaseLoggerWrapper(GetType());
            var actual = sut.FormatException(exception);

            actual.Should().Be("    Griffin.Tests.Logging.Loggers.ExceptionWithProperty2: Exception of type 'Griffin.Tests.Logging.Loggers.ExceptionWithProperty2' was thrown.\r\n    [UserId='10']\r\n");
        }

    }

    public class ExceptionWithProperty : Exception
    {
        public int UserId { get; set; }
    }

    public class ExceptionWithProperty2 : Exception
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
    }
}
