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
    public class IocQueryBusTests
    {
        [Fact]
        public void must_get_container_to_work_successfully()
        {

            Action x = () => new IocQueryBus(null);

            x.Should().Throw<ArgumentNullException>();
        }


        [Fact]
        public void may_only_have_one_query_handler_to_avoid_ambiguity()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IQueryHandler<TestQuery, string>>(),
                    Substitute.For<IQueryHandler<TestQuery, string>>()
                });

            var sut = new IocQueryBus(container);
            Action x = () => sut.QueryAsync(new TestQuery()).Wait();

            x.Should().Throw<OnlyOneHandlerAllowedException>();
        }

        [Fact]
        public void must_have_one_handler_to_be_able_to_execute_query()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new IQueryHandler<TestQuery, string>[0]);

            var sut = new IocQueryBus(container);
            Action x = () => sut.QueryAsync(new TestQuery()).Wait();

            x.Should().Throw<CqsHandlerMissingException>();
        }

        [Fact]
        public async Task should_dispose_scope_when_done()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]
                {
                    Substitute.For<IQueryHandler<TestQuery, string>>(),
                });

            var sut = new IocQueryBus(container);
            await sut.QueryAsync(new TestQuery());

            scope.Received().Dispose();
        }

        [Fact]
        public async Task do_not_throw_target_invocation_exception_upon_failure()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var query = new TestQuery();
            var handler = Substitute.For<IQueryHandler<TestQuery, string>>();
            Exception actual = null;
            container.CreateScope().Returns(scope);
            handler.When(x => x.ExecuteAsync(query)).Do(x => { throw new DataException(); });
            scope.ResolveAll(null).ReturnsForAnyArgs(new[] {handler});

            var sut = new IocQueryBus(container);
            try
            {
                await sut.QueryAsync(query);
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
            var handler = Substitute.For<IQueryHandler<TestQuery, string>>();
            var query = new TestQuery();
            container.CreateScope().Returns(scope);
            scope.ResolveAll(null).ReturnsForAnyArgs(new[]{handler});

            var sut = new IocQueryBus(container);
            await sut.QueryAsync(query);

            handler.Received().ExecuteAsync(query);
        }

        public class TestQuery : Query<string>
        {
            
        }
    }
}
