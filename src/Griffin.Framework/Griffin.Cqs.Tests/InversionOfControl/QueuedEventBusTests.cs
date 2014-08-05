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
    public class QueuedEventBusTests
    {
        [Fact]
        public void must_get_innerBus_to_work_successfully()
        {

            Action x = () => new QueuedEventBus(null, 1);

            x.ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public async Task works_with_multiple_handlers()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var subscriber1 = Substitute.For<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>();
            var subscriber2 = Substitute.For<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>();
            var evt = new IocEventBusTests.TestEvent();
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>()
                .Returns(new[] { subscriber1, subscriber2 });
            var innerBus = new IocEventBus(container);

            var sut = new QueuedEventBus(innerBus, 1);
            await sut.PublishAsync(evt);
            await sut.ExecuteJobAsync();

            subscriber1.Received().HandleAsync(evt);
            subscriber2.Received().HandleAsync(evt);
        }

        [Fact]
        public async Task one_failing_handler_should_not_abort_others()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var evt = new IocEventBusTests.TestEvent();
            var successHandler = new IocEventBusTests.SuccessfulHandler();
            Exception actual = null;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>()
                .Returns(new IApplicationEventSubscriber<IocEventBusTests.TestEvent>[]
                {
                    new IocEventBusTests.FailingHandler(),
                    successHandler
                });
            var inner = new IocEventBus(container);

            var sut = new QueuedEventBus(inner, 1);
            try
            {
                await sut.PublishAsync(evt);
                await sut.ExecuteJobAsync();
            }
            catch (Exception exception)
            {
                actual = exception;
            }


            successHandler.IsCalled.Should().BeTrue();
            actual.Should().BeOfType<AggregateException>();
        }

        [Fact]
        public async Task should_trigger_event_upon_successful_completion()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>()
                            .Returns(new[]
                {
                    Substitute.For<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>(),
                });
            var inner = new IocEventBus(container);

            var sut = new QueuedEventBus(inner, 1);
            sut.EventPublished += (sender, args) => actual = true;
            await sut.PublishAsync(new IocEventBusTests.TestEvent());
            await sut.ExecuteJobAsync();

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task event_should_report_Failure_if_one_handler_throws_an_exception()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            container.CreateScope().Returns(scope);
            scope.ResolveAll<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>()
                            .Returns(new[]
                {
                    Substitute.For<IApplicationEventSubscriber<IocEventBusTests.TestEvent>>(),
                    new IocEventBusTests.FailingHandler(), 
                });
            var inner = new IocEventBus(container);

            var sut = new QueuedEventBus(inner, 1);
            sut.EventPublished += (sender, args) => actual = args.Successful;
            try
            {
                sut.PublishAsync(new IocEventBusTests.TestEvent());
            }
            catch { }

            actual.Should().BeFalse();
        }

        public class SuccessfulHandler : IApplicationEventSubscriber<IocEventBusTests.TestEvent>
        {
            public async Task HandleAsync(IocEventBusTests.TestEvent e)
            {
                Console.WriteLine("invoked");
                IsCalled = true;
            }

            public bool IsCalled { get; set; }
        }

        public class FailingHandler : IApplicationEventSubscriber<IocEventBusTests.TestEvent>
        {
            public async Task HandleAsync(IocEventBusTests.TestEvent e)
            {
                throw new Exception("Ooops");
            }
        }

        public class TestEvent : ApplicationEvent
        {

        }
    }
}
