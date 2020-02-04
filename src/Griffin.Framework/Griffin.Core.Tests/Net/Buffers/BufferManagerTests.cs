using System;
using FluentAssertions;
using Griffin.Net.Buffers;
using Xunit;

namespace Griffin.Core.Tests.Net.Buffers
{
    public class BufferManagerTests
    {

        [Fact]
        public void Dequeue_too_many()
        {
            var sut = new BufferManager(2, 10) { PainThreshold = 20 };

            sut.Dequeue();
            sut.Dequeue();
            Action actual = () => sut.Dequeue();

            actual.Should().Throw<PoolEmptyException>();
        }

        [Fact]
        public void Dequeue_one()
        {
            var sut = new BufferManager(10, 2);

            var actual = sut.Dequeue();

            actual.Capacity.Should().Be(2);
            actual.Offset.Should().Be(0, "because we are using a queue");
        }

        [Fact]
        public void return_to_pool__verify_that_it_got_enqueued()
        {
            var sut = new BufferManager(10, 2);
            var buffer = sut.Dequeue();

            sut.Enqueue(buffer);

            sut.Dequeue();
            sut.Dequeue();
        }

        [Fact]
        public void return_to_pool_directly()
        {
            var sut = new BufferManager(10, 2);
            var buffer = sut.Dequeue();
            sut.Enqueue(buffer);

            sut.Dequeue();
            sut.Dequeue();
        }
    }
}
