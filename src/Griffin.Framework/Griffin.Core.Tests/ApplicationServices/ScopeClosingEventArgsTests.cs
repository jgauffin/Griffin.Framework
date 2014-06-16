using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Container;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    public class ScopeClosingEventArgsTests
    {
        [Fact]
        public void the_constructor_should_assign_scope_to_the_property()
        {
            var scope = Substitute.For<IContainerScope>();

            var sut = new ScopeClosingEventArgs(scope, true);

            sut.Scope.Should().BeSameAs(scope);
        }

        [Fact]
        public void the_constructor_should_assign_success_to_the_property()
        {
            var scope = Substitute.For<IContainerScope>();

            var sut = new ScopeClosingEventArgs(scope, true);

            sut.Successful.Should().BeTrue();
        }

        [Fact]
        public void the_constructor_should_assign_failure_to_the_property()
        {
            var scope = Substitute.For<IContainerScope>();

            var sut = new ScopeClosingEventArgs(scope, false);

            sut.Successful.Should().BeFalse();
        }

    }
}