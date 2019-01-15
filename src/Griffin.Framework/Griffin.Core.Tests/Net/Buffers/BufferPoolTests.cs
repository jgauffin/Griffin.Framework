using System;
using FluentAssertions;
using Griffin.Net.Buffers;
using Xunit;

namespace Griffin.Core.Tests.Net.Buffers
{
    public class BufferPoolTests
    {
    
        [Fact]
        public void pop_too_many()
        {
            var sut = new BufferSlicePool(10, 2);

            sut.Pop();
            sut.Pop();
            Action actual = () => sut.Pop();

            actual.Should().Throw<PoolEmptyException>();
        }

        [Fact]
        public void pop_one()
        {
            var sut = new BufferSlicePool(10, 2);

            var actual = sut.Pop();

            actual.Capacity.Should().Be(10);
            actual.Offset.Should().Be(10, "internal implementation is a stack, last buffer is at pos 10");
        }

        [Fact]
        public void return_to_pool__verify_that_it_got_enqueued()
        {
            var sut = new BufferSlicePool(10, 2);
            var buffer = sut.Pop();
            
            sut.Push(buffer);

            sut.Pop();
            sut.Pop();
        }

        [Fact]
        public void return_to_pool_directly()
        {
            var sut = new BufferSlicePool(10, 2);
            var buffer = sut.Pop();
            sut.Push(buffer);

            sut.Pop();
            sut.Pop();
        }
    }
}
