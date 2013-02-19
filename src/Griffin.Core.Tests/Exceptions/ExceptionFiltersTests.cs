using System;
using NSubstitute;
using Xunit;

namespace Griffin.Framework.Tests.Exceptions
{
    public class ExceptionFiltersTests
    {
        [Fact]
        public void NoFilters()
        {
            ExceptionFilters.Clear();
            ExceptionFilters.Trigger(new ExceptionFilterContext(new Exception("This is an exception. Take care with it's details. They may not exist for ever. Whatever.")));
        }

        [Fact]
        public void OneFilter()
        {
            var filter = Substitute.For<IExceptionFilter>();
            var context =
                new ExceptionFilterContext(
                    new Exception(
                        "This is an exception. Take care with it's details. They may not exist for ever. Whatever."));

            ExceptionFilters.Register(filter);
            ExceptionFilters.Trigger(context);

            filter.Received().Handle(context);
        }
    }
}
