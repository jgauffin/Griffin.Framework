using FluentAssertions;
using Griffin.Logging.Providers;
using Xunit;

namespace Griffin.Core.Tests.Logging
{
    public class NamespaceFilterTests
    {
        [Fact]
        public void child_ns_is_configured_to_be_allowed__so_allow_it()
        {
            var sut = new NamespaceFilter();
            sut.Allow("Griffin", true);
            var actual = sut.IsSatisfiedBy(typeof(NoFilter));

            actual.Should().BeTrue();
        }

        [Fact]
        public void child_ns_is_configured_to_NOT_be_allowed__so_dont_allow_it()
        {
            var sut = new NamespaceFilter();
            sut.Allow("Griffin", false);
            var actual = sut.IsSatisfiedBy(typeof(NoFilter));

            actual.Should().BeFalse();
        }

        [Fact]
        public void no_filters_means_that_everything_is_revoked()
        {
            var sut = new NamespaceFilter();
            var actual = sut.IsSatisfiedBy(typeof(NoFilter));

            actual.Should().BeFalse();
        }

        [Fact]
        public void allow_root_but_reject_specific_child__make_sure_that_the_root_is_allowed()
        {
            var sut = new NamespaceFilter();
            sut.Revoke(typeof(NoFilter).Namespace, false);
            sut.Allow("Griffin", true);
            var actual = sut.IsSatisfiedBy(typeof(GuidFactory));

            actual.Should().BeTrue();
        }

        [Fact]
        public void allow_root_but_reject_specific_child()
        {
            var sut = new NamespaceFilter();
            sut.Revoke(typeof (NoFilter).Namespace, false);
            sut.Allow("Griffin", true);
            var actual = sut.IsSatisfiedBy(typeof(NoFilter));

            actual.Should().BeFalse("because a specific filter was set");
        }
    }
}