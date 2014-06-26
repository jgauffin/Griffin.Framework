using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Cqs.InversionOfControl;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class MemoryQueueTests
    {
        [Fact]
        public async Task dequeue_on_empty_queue_should_return_default_item()
        {
            
            var sut = new MemoryQueue<object>();
            var actual = await sut.DequeueAsync();

            actual.Should().BeNull();
        }

        [Fact]
        public async Task dequeue_item_should_work()
        {
            var sut = new MemoryQueue<object>();
            var expected = new object();
            await sut.EnqueueAsync(expected);

            var actual = await sut.DequeueAsync();

            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task enqueue_should_put_item_on_queue()
        {
            var sut = new MemoryQueue<object>();
            var expected = new object();

            await sut.EnqueueAsync(expected);
            var actual = await sut.DequeueAsync();

            actual.Should().BeSameAs(expected);
        }
    }
}
