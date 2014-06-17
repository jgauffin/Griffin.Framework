using System;
using FluentAssertions;
using Griffin.ApplicationServices;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class BackgroundJobFailedEventArgsTests
    {
        [Fact]
        public void konstruktorn_ska_initiera_våra_properties()
        {
            var job = Substitute.For<IBackgroundJob>();
            var exception = new Exception();

            var args = new BackgroundJobFailedEventArgs(job, exception);

            args.Job.Should().Be(job);
            args.Exception.Should().Be(exception);
        }
    }
}
