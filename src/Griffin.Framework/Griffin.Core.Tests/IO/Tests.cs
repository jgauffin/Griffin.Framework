using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.IO;
using Xunit;

namespace Griffin.Core.Tests.IO
{
    public class Tests
    {
        [Fact]
        public void test()
        {


            var sut = new PersistentCircularIndex(Path.GetTempFileName(), 36, 5);
            sut.Enqueue("1");
            sut.Enqueue("2");
            sut.Enqueue("3");
            sut.Enqueue("4");
            sut.Dequeue().Should().Be("1");
            sut.Enqueue("5");
            sut.Enqueue("6");
            sut.Dequeue().Should().Be("2");
            sut.Dequeue().Should().Be("3");
            sut.Dequeue().Should().Be("4");
            sut.Enqueue("7");
            sut.Dequeue().Should().Be("5");
            sut.Dequeue().Should().Be("6");
            sut.Dequeue().Should().Be("7");
            sut.Dequeue().Should().BeNull();
        }

        [Fact]
        public void circulate()
        {


            var sut = new PersistentCircularIndex(Path.GetTempFileName(), 36, 3);
            sut.Enqueue("1");
            sut.Enqueue("2");
            sut.Enqueue("3");
            sut.Dequeue().Should().Be("1");
            sut.Enqueue("4");
            sut.Dequeue().Should().Be("2");
            sut.Dequeue().Should().Be("3");
            sut.Enqueue("5");
            sut.Enqueue("6");
            sut.Dequeue().Should().Be("4");
            sut.Dequeue().Should().Be("5");
            sut.Dequeue().Should().Be("6");
            sut.Dequeue().Should().BeNull();
            sut.Dequeue().Should().BeNull();
        }

        [Fact]
        public void full_queue()
        {


            var sut = new PersistentCircularIndex(Path.GetTempFileName(), 36, 3);
            sut.Enqueue("1");
            sut.Enqueue("2");
            sut.Enqueue("3");
            Action actual = () => sut.Enqueue("4");

            actual.ShouldThrow<InvalidOperationException>();
        }

    }
}
