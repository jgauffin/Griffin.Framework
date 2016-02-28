using System;
using System.Threading.Tasks;
using DotNetCqs;
using FluentAssertions;
using Griffin.Container;
using Griffin.Cqs.InversionOfControl;
using NSubstitute;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class SeparateScopesSeparateScopesIocEventBusTests
    {
        [Fact]
        public void must_get_container_to_be_fully_functional()
        {
            var registry = new EventHandlerRegistry();

            Action x = () => new SeparateScopesIocEventBus(null, registry);

            x.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void must_have_a_registry_to_be_fully_functional()
        {
            var container = Substitute.For<IContainer>();

            Action x = () => new SeparateScopesIocEventBus(container, null);

            x.ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public async Task should_create_one_scope_per_handler()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope1 = Substitute.For<IContainerScope>();
            var scope2 = Substitute.For<IContainerScope>();
            var subscriber1 = Substitute.For<IApplicationEventSubscriber<TestEvent>>();
            var subscriber2 = Substitute.For<IApplicationEventSubscriber<TestEvent>>();
            var evt = new TestEvent();
            registry.Map<TestEvent>(subscriber1.GetType());
            registry.Map<TestEvent>(subscriber2.GetType());
            container.CreateScope().Returns(scope1, scope2);
            scope1.Resolve(subscriber1.GetType()).Returns(subscriber1);
            scope2.Resolve(subscriber2.GetType()).Returns(subscriber2);

            var sut = new SeparateScopesIocEventBus(container, registry);
            await sut.PublishAsync(evt);

            subscriber1.Received().HandleAsync(evt);
            subscriber2.Received().HandleAsync(evt);
        }

        [Fact]
        public async Task should_work_with_just_one_handler()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope1 = Substitute.For<IContainerScope>();
            var subscriber1 = Substitute.For<IApplicationEventSubscriber<TestEvent>>();
            var evt = new TestEvent();
            registry.Map<TestEvent>(subscriber1.GetType());
            container.CreateScope().Returns(scope1);
            scope1.Resolve(subscriber1.GetType()).Returns(subscriber1);

            var sut = new SeparateScopesIocEventBus(container, registry);
            await sut.PublishAsync(evt);

            subscriber1.Received().HandleAsync(evt);
        }

        [Fact]
        public async Task one_failing_handler_should_not_abort_others()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope1 = Substitute.For<IContainerScope>();
            var scope2 = Substitute.For<IContainerScope>();
            var evt = new TestEvent();
            var successHandler = new SuccessfulHandler();
            var failingHandler = new FailingHandler();
            Exception actual = null;
            registry.Map<TestEvent>(failingHandler.GetType());
            registry.Map<TestEvent>(successHandler.GetType());
            container.CreateScope().Returns(scope1, scope2);
            scope1.Resolve(failingHandler.GetType()).Returns(failingHandler);
            scope2.Resolve(successHandler.GetType()).Returns(successHandler);

            var sut = new SeparateScopesIocEventBus(container, registry);
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
        public async Task works_without_subscribers_since_there_is_no_coupling_between_the_publisher_and_subscribers()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();

            var sut = new SeparateScopesIocEventBus(container, registry);
            await sut.PublishAsync(new TestEvent());
        }

        [Fact]
        public async Task should_dispose_scope_when_done()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler = new SuccessfulHandler();
            registry.Map<TestEvent>(handler.GetType());
            scope.Resolve(handler.GetType()).Returns(handler);
            container.CreateScope().Returns(scope);

            var sut = new SeparateScopesIocEventBus(container, registry);
            await sut.PublishAsync(new TestEvent());

            scope.Received().Dispose();
        }

        [Fact]
        public async Task should_trigger_event_upon_successful_completion()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var actual = false;
            var handler = new SuccessfulHandler();
            registry.Map<TestEvent>(handler.GetType());
            scope.Resolve(handler.GetType()).Returns(handler);
            container.CreateScope().Returns(scope);

            var sut = new SeparateScopesIocEventBus(container, registry);
            sut.EventPublished += (sender, args) => actual = true;
            await sut.PublishAsync(new TestEvent());

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task event_should_report_Failure_if_one_handler_throws_an_exception()
        {
            var registry = new EventHandlerRegistry();
            var container = Substitute.For<IContainer>();
            var scope1 = Substitute.For<IContainerScope>();
            var scope2 = Substitute.For<IContainerScope>();
            var successHandler = new SuccessfulHandler();
            var failingHandler = new FailingHandler();
            bool actual = false;
            registry.Map<TestEvent>(failingHandler.GetType());
            registry.Map<TestEvent>(successHandler.GetType());
            container.CreateScope().Returns(scope1, scope2);
            scope1.Resolve(failingHandler.GetType()).Returns(failingHandler);
            scope2.Resolve(successHandler.GetType()).Returns(successHandler);

            var sut = new SeparateScopesIocEventBus(container, registry);
            sut.EventPublished += (sender, args) => actual = args.Successful;
            try
            {
                sut.PublishAsync(new TestEvent());
            }
            catch
            {
            }

            actual.Should().BeFalse();
        }

        public class SuccessfulHandler : IApplicationEventSubscriber<TestEvent>
        {
            public bool IsCalled { get; set; }

            public async Task HandleAsync(TestEvent e)
            {
                Console.WriteLine("invoked");
                IsCalled = true;
            }
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