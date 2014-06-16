using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Container;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{

    public class ScopeCreatedEventArgsTests
    {
        [Fact]
        public void the_constructor_should_assign_the_value_to_the_property()
        {
            var scope = Substitute.For<IContainerScope>();

            var sut = new ScopeCreatedEventArgs(scope);

            sut.Scope.Should().BeSameAs(scope);
        }

    }
}
