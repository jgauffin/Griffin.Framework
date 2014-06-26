using System;
using System.Collections.Generic;
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
    public class IocCommandBusTests
    {
        [Fact]
        public void must_get_container_to_work_successfully()
        {

            Action x = () => new IocCommandBus(null);

            x.ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public void may_only_have_one_command_handler_to_avoid_ambiguity()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>()
                .Returns(new[]
                {
                    Substitute.For<ICommandHandler<TestCommand>>(),
                    Substitute.For<ICommandHandler<TestCommand>>()
                });

            var sut = new IocCommandBus(container);
            Action x = () => sut.ExecuteAsync(new TestCommand()).Wait();

            x.ShouldThrow<OnlyOneHandlerAllowedException>();
        }

        [Fact]
        public void must_have_one_handler_to_be_able_to_execute_command()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>()
                .Returns(new ICommandHandler<TestCommand>[0]);

            var sut = new IocCommandBus(container);
            Action x = () => sut.ExecuteAsync(new TestCommand()).Wait();

            x.ShouldThrow<CqsHandlerMissingException>();
        }

        [Fact]
        public async Task should_dispose_scope_wWhen_done()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>()
                            .Returns(new[]
                {
                    Substitute.For<ICommandHandler<TestCommand>>(),
                });

            var sut = new IocCommandBus(container);
            await sut.ExecuteAsync(new TestCommand());

            scope.Received().Dispose();
        }

        [Fact]
        public async Task handler_is_being_invoked()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler = Substitute.For<ICommandHandler<TestCommand>>();
            var command = new TestCommand();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>().Returns(new[]{handler});

            var sut = new IocCommandBus(container);
            await sut.ExecuteAsync(command);

            handler.Received().ExecuteAsync(command);
        }

        [Fact]
        public async Task should_trigger_event_upon_successful_completion()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>()
                            .Returns(new[]
                {
                    Substitute.For<ICommandHandler<TestCommand>>(),
                });

            var sut = new IocCommandBus(container);
            sut.CommandInvoked += (sender, args) => actual = true;
            await sut.ExecuteAsync(new TestCommand());

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task should_NOT_trigger_event_upon_failure()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<ICommandHandler<TestCommand>>()
                            .Returns(new[]
                {
                    Substitute.For<ICommandHandler<TestCommand>>(),
                    Substitute.For<ICommandHandler<TestCommand>>(),
                });

            var sut = new IocCommandBus(container);
            sut.CommandInvoked += (sender, args) => actual = true;
            try
            {
                sut.ExecuteAsync(new TestCommand());
            }
            catch{}

            actual.Should().BeFalse();
        }


        public class TestCommand : Command
        {
            
        }
    }
}
