﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Container;
using NSubstitute;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class IocEventBusTests
    {

        [Fact]
        public void Must_get_container_to_work_successfully()
        {

            Action x = () => new IocEventBus(null);

            x.Should().Throw<ArgumentNullException>();
        }


        [Fact]
        public async Task Works_with_multiple_handlers()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var subscriber1 = Substitute.For<IApplicationEventSubscriber<TestEvent>>();
            var subscriber2 = Substitute.For<IApplicationEventSubscriber<TestEvent>>();
            var evt = new TestEvent();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                .Returns(new[] { subscriber1, subscriber2 });

            var sut = new IocEventBus(container);
            await sut.PublishAsync(evt);

            subscriber1.Received().HandleAsync(evt);
            subscriber2.Received().HandleAsync(evt);
        }

        [Fact]
        public async Task One_failing_handler_should_not_abort_others()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var evt = new TestEvent();
            var successHandler = new SuccessfulHandler();
            Exception actual = null;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                .Returns(new IApplicationEventSubscriber<TestEvent>[]
                {
                    new FailingHandler(),
                    successHandler
                });

            var sut = new IocEventBus(container);
            try
            {
                await sut.PublishAsync(evt);
            }
            catch (Exception exception)
            {
                actual = exception;
            }


            successHandler.IsCalled.Should().BeTrue();
            actual.Should().BeOfType<AggregateException>();
        }

        [Fact]
        public async Task Works_without_subscribers_since_there_is_no_coupling_between_the_publisher_and_subscribers()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                .Returns(new IApplicationEventSubscriber<TestEvent>[0]);

            var sut = new IocEventBus(container);
            await sut.PublishAsync(new TestEvent());

        }

        [Fact]
        public async Task Should_dispose_scope_when_done()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                            .Returns(new[]
                {
                    Substitute.For<IApplicationEventSubscriber<TestEvent>>(),
                });

            var sut = new IocEventBus(container);
            await sut.PublishAsync(new TestEvent());

            scope.Received().Dispose();
        }

        [Fact]
        public async Task Should_trigger_event_upon_successful_completion()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                            .Returns(new[]
                {
                    Substitute.For<IApplicationEventSubscriber<TestEvent>>(),
                });

            var sut = new IocEventBus(container);
            sut.EventPublished += (sender, args) => actual = true;
            await sut.PublishAsync(new TestEvent());

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task Event_should_report_Failure_if_one_handler_throws_an_exception()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<TestEvent>>()
                            .Returns(new[]
                {
                    Substitute.For<IApplicationEventSubscriber<TestEvent>>(),
                    new FailingHandler(), 
                });

            var sut = new IocEventBus(container);
            sut.EventPublished += (sender, args) => actual = args.Successful;
            try
            {
                sut.PublishAsync(new TestEvent());
            }
            catch { }

            actual.Should().BeFalse();
        }

        public class SuccessfulHandler : IApplicationEventSubscriber<TestEvent>
        {
            public async Task HandleAsync(TestEvent e)
            {
                Console.WriteLine("invoked");
                IsCalled = true;
            }

            public bool IsCalled { get; set; }
        }

        public class FailingHandler : IApplicationEventSubscriber<TestEvent>
        {
            public async Task HandleAsync(TestEvent e)
            {
                throw new Exception("Ooops");
            }
        }

        public class TestEvent : ApplicationEvent
        {

        }
    }
}
