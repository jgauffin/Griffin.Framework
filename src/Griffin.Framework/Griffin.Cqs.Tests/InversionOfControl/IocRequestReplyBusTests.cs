using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using FluentAssertions;
using Griffin.Container;
using Griffin.Cqs.InversionOfControl;
using NSubstitute;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class IocRequestReplyBusTests
    {
        [Fact]
        public void must_get_container_to_work_successfully()
        {

            Action x = () => new IocRequestReplyBus(null);

            x.ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public void may_only_have_one_request_handler_to_avoid_ambiguity()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IRequestHandler<TestRequest, string>>(),
                    Substitute.For<IRequestHandler<TestRequest, string>>()
                });

            var sut = new IocRequestReplyBus(container);
            Action x = () => sut.ExecuteAsync(new TestRequest()).Wait();

            x.ShouldThrow<OnlyOneHandlerAllowedException>();
        }

        [Fact]
        public void must_have_one_handler_to_be_able_to_execute_request()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new IRequestHandler<TestRequest, string>[0]);

            var sut = new IocRequestReplyBus(container);
            Action x = () => sut.ExecuteAsync(new TestRequest()).Wait();

            x.ShouldThrow<CqsHandlerMissingException>();
        }

        [Fact]
        public async Task should_dispose_scope_when_done()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IRequestHandler<TestRequest, string>>(),
                });

            var sut = new IocRequestReplyBus(container);
            await sut.ExecuteAsync(new TestRequest());

            scope.Received().Dispose();
        }

        [Fact]
        public async Task do_not_throw_target_invocation_exception_upon_failure()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var request = new TestRequest();
            var handler = Substitute.For<IRequestHandler<TestRequest, string>>();
            Exception actual = null;
            container.CreateScope().Returns(scope);
            handler.When(x => x.ExecuteAsync(request)).Do(x => { throw new DataException(); });
            scope.ResolveAll(null).ReturnsForAnyArgs(new[] {handler});

            var sut = new IocRequestReplyBus(container);
            try
            {
                await sut.ExecuteAsync(request);
            }
            catch (Exception exception)
            {
                actual = exception;
            }

            actual.Should().BeOfType<DataException>();
        }

        [Fact]
        public async Task handler_is_being_invoked()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler = Substitute.For<IRequestHandler<TestRequest, string>>();
            var request = new TestRequest();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]{handler});

            var sut = new IocRequestReplyBus(container);
            await sut.ExecuteAsync(request);

            handler.Received().ExecuteAsync(request);
        }



        [Fact]
        public async Task should_trigger_event_upon_successful_completion()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null)
                            .ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IRequestHandler<TestRequest, string>>(),
                });

            var sut = new IocRequestReplyBus(container);
            sut.RequestInvoked += (sender, args) => actual = true;
            await sut.ExecuteAsync(new TestRequest());

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task should_NOT_trigger_event_upon_failure()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null)
                            .ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IRequestHandler<TestRequest, string>>(),
                    Substitute.For<IRequestHandler<TestRequest, string>>(),
                });

            var sut = new IocRequestReplyBus(container);
            sut.RequestInvoked += (sender, args) => actual = true;
            try
            {
                sut.ExecuteAsync(new TestRequest());
            }
            catch { }

            actual.Should().BeFalse();
        }

        public class TestRequest : Request<string>
        {
            
        }
    }
}
