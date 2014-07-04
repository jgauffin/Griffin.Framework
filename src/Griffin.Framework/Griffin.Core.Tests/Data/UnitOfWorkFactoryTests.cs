using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data
{
    public class UnitOfWorkFactoryTests
    {
        [Fact]
        public void no_factory_assigned_should_throw_an_exception_explaining_that()
        {
            UnitOfWorkFactory.SetFactoryMethod(UnitOfWorkFactory.ClearAssignment);

            Action actual = () => UnitOfWorkFactory.Create();

            actual.ShouldThrow<InvalidOperationException>().And.Message.Should().Contain("SetFactoryMethod()");
        }

        [Fact]
        public void call_factory_when_Create_is_called()
        {
            var uow = Substitute.For<IUnitOfWork>();

            UnitOfWorkFactory.SetFactoryMethod(() => uow);
            var actual = UnitOfWorkFactory.Create();

            actual.Should().Be(uow);
        }
    }
}
