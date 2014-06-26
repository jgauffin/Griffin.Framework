using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Container;
using Griffin.Cqs.InversionOfControl;
using NSubstitute;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class CommandInvokedEventArgsTests
    {
        [Fact]
        public void Command_must_be_specified()
        {
            var scope = Substitute.For<IContainerScope>();

            Action actual = () => new CommandInvokedEventArgs(scope, null);

            actual.ShouldThrow<ArgumentNullException>();
        }
    }
}
