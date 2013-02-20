using System;
using Griffin.Framework.Logging;
using NSubstitute;
using Xunit;

namespace Griffin.Framework.Tests.Logging
{
    public class LogManagerTests
    {
        [Fact]
        public void Assign_Null()
        {
            Assert.Throws<ArgumentNullException>(() => LogManager.Assign(null));
        }


        [Fact]
        public void Assign_Ok()
        {
            LogManager.Assign(Substitute.For<ILogManager>());
        }

        [Fact]
        public void Assign_Twice()
        {
            LogManager.Assign(Substitute.For<ILogManager>());
            LogManager.Assign(Substitute.For<ILogManager>());
        }

        [Fact]
        public void GetLogger()
        {
            Assert.NotNull(LogManager.GetLogger(GetType()));
        }

        [Fact]
        public void GetLogger_Generic()
        {
            Assert.NotNull(LogManager.GetLogger<string>());
        }

    }
}
