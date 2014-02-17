using FluentAssertions;
using Griffin.Logging.Loggers;
using Griffin.Logging.Providers;
using Xunit;

namespace Griffin.Core.Tests.Logging.Providers
{
    public class LogProviderTests
    {
        [Fact]
        public void return_the_only_added_logger()
        {
            var sut = new LogProvider();
            sut.Add(new NullLogger());

            var logger = sut.GetLogger(GetType());

            logger.Should().BeOfType<NullLogger>();
        }

        [Fact]
        public void return_all_loggers_as_a_composite_if_no_filters_are_used()
        {
            var sut = new LogProvider();
            sut.Add(new NullLogger());
            sut.Add(new NullLogger());

            var logger = sut.GetLogger(GetType());

            logger.Should().BeOfType<CompositeLogger>();
        }

        [Fact]
        public void only_return_one_logger_if_the_filter_is_not_ok()
        {
            var sut = new LogProvider();
            sut.Add(new NullLogger());
            sut.Add(new NullLogger(), new NamespaceFilter(revokedIncludingChildNamespaces:"Griffin"));

            var logger = sut.GetLogger(GetType());

            logger.Should().BeOfType<NullLogger>();
        }

        [Fact]
        public void return_composite_if_filter_is_ok()
        {
            var sut = new LogProvider();
            sut.Add(new NullLogger());
            sut.Add(new NullLogger(), new NamespaceFilter(allowedIncludingChildNamespaces: "Griffin"));

            var logger = sut.GetLogger(GetType());

            logger.Should().BeOfType<CompositeLogger>();
        }
    }
}
